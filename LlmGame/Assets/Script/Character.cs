using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;
    [SerializeField] public bool animationFinished = false;
    public BattleManager battleManager;

    [Header("Basic Info")]
    public string characterName;
    public CharacterType characterType;
    public GameObject sprite;
    [TextArea] public string description;

    [Header("Stats")]
    public int attack;
    public int defense;
    public int focus;
    public int maxHP;
    public int maxMP;
    public int speed;
    public int maxShield;

    [Header("Resource")]
    public int money;

    [Header("Body Parts")]
    public BodyPartConfig bodyPartConfig;
    [HideInInspector] public List<BodyPartData> bodyParts = new List<BodyPartData>();

    [Header("Actions")]
    public List<CharacterActionData> availableActions = new List<CharacterActionData>();
    public CharacterActionData selectedAction;
    public int pendingDamage;
    public List<float> damagePortions = new List<float>();
    public Character damageTarget;
    public int currentHitIndex = 0;

    [Header("Status Effects")]
    [SerializeField] public List<TurnStatusEffect> activeStatusEffects = new List<TurnStatusEffect>();

    [Header("Critical")]
    public bool isCritical;

    [Header("Skills")]
    public List<DamageModifierSkill> damageModifierSkills = new List<DamageModifierSkill>();
    public DamageModifierSkill currentSkill;
    public bool isUsingUltimateSkill = false;

    [Header("Runtime")]
    [SerializeField] public int currentHP;
    [SerializeField] public int currentMP;
    [SerializeField] public int currentshield;
    public List<MonoBehaviour> runtimePassiveBehaviors = new();
    public float turnGauge = 0f;

    [Header("Inventory")]
    public List<Item> inventoryItems;

    [Header("Equipment")]
    public Weapon leftHandWeapon;
    public Weapon rightHandWeapon;

    [Header("Active Items")]
    public List<Item> activeItem;
    public bool isUsingConsumeTurnItem;

    public PossibilityPool possibilityPool { get; private set; }
    [SerializeField] public List<PassiveItemData> equippedPassiveItems;
    private Dictionary<string, int> customIntData = new();

    public virtual void Awake()
    {
        battleManager = FindAnyObjectByType<BattleManager>();
        possibilityPool = new PossibilityPool();
        animator = GetComponent<Animator>();
        currentHP = maxHP;
        currentMP = maxMP;
        currentshield = maxShield;

        if (bodyPartConfig != null)
        {
            bodyParts = bodyPartConfig.GenerateBodyParts();
        }
    }

    private void Start()
    {
        // Find BattleManager in the scene (or assign manually if you prefer)
        battleManager = FindObjectOfType<BattleManager>();
    }

    public string GetBodyPartStatus()
    {
        if (bodyParts == null || bodyParts.Count == 0)
            return "No body parts assigned.";

        List<string> statusLines = new();

        foreach (var part in bodyParts)
        {
            string partName = part.type.ToString();
            string partState = part.state.ToString();
            int currentHP = part.health;
            int maxHP = part.maxHealth;

            statusLines.Add($"{partName}: {currentHP}/{maxHP} HP - {partState} - {part.linkedWeakPoint}");
        }

        return string.Join("\n", statusLines);
    }

    public virtual void TakeDamage(int dmg)
    {
        int finalDamage = Mathf.Max(dmg - defense, 0);
        currentHP -= finalDamage;
        if (currentHP < 0) currentHP = 0;

        if (currentHP <= 0)
        {
            OnDeath();
        }
    }

    public virtual void OnDeath()
    {
        Debug.Log($"{characterName} has died!");

        // 1️⃣ Notify components on this character (self-listeners)
        foreach (var listener in GetComponents<IDeathListener>())
        {
            listener.OnDeath(this);
        }

        // 2️⃣ Also notify the Player’s passive item listeners
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            foreach (var itemData in player.equippedPassiveItems)
            {
                if (itemData.itemPrefab == null) continue;

                var deathListener = itemData.itemPrefab.GetComponent<IDeathListener>();
                if (deathListener != null)
                {
                    deathListener.OnDeath(this);
                }
            }
        }

    }

    private void OnMouseDown()
    {
        if (battleManager != null && battleManager.isActionPhase && battleManager.currentActingCharacter is Player)
        {
            if (this is Character && this.IsAlive())
            {
                battleManager.PlayerSelectedTarget(this as Character);
            }
        }
    }

    public virtual bool IsAlive()
    {
        return currentHP > 0;
    }

    public virtual bool IsDead()
    {
        return currentHP <= 0;
    }

    public string GetStatus()
    {
        return $"HP: {currentHP}/{maxHP}, MP: {currentMP}/{maxMP}";
    }

    public void OnAnimationComplete()
    {
        Debug.Log($"{characterName} animation complete (via Event)");
        animationFinished = true;
    }

    public void ApplyDamageAtHit()
    {
        if (damageTarget == null || !damageTarget.IsAlive())
        {
            Debug.LogWarning("damageTarget is null or dead.");
            return;
        }

        if (selectedAction == null || selectedAction.hitEffects == null || currentHitIndex >= selectedAction.hitEffects.Count)
        {
            Debug.LogWarning($"{characterName} has no more hitEffects left or invalid index.");
            return;
        }

        var hitData = selectedAction.hitEffects[currentHitIndex];
        float portion = hitData.damagePortion;
        int thisHitDamage = Mathf.RoundToInt(pendingDamage * portion);

        // ✅ BEFORE damage: Offensive passives from attacker
        if (this is Player playerAttackerBefore)
        {
            foreach (var behavior in playerAttackerBefore.runtimePassiveBehaviors)
            {
                if (behavior is IDamageReaction reaction)
                    reaction.OnBeforeDamage(playerAttackerBefore, damageTarget, ref thisHitDamage);
            }
        }

        // 💥 Apply damage
        damageTarget.TakeDamage(thisHitDamage);

        // ✅ 💉 Consume BloodRushCore if crit applied
        if (this is Player playerAttackerCrit && playerAttackerCrit.isCritical)
        {
            foreach (var behavior in playerAttackerCrit.runtimePassiveBehaviors)
            {
                if (behavior is BloodRushCore brc && brc.IsReady())
                {
                    brc.Consume();
                    Debug.Log("🩸 BloodRushCore consumed after critical hit.");
                }
            }
        }

        // 💡 AFTER damage: Passive reactions from damageTarget
        if (damageTarget is Player playerTargetAfter)
        {
            foreach (var behavior in playerTargetAfter.runtimePassiveBehaviors)
            {
                if (behavior is IDamageReaction reaction)
                    reaction.OnBeforeDamage(playerTargetAfter, damageTarget, ref thisHitDamage);
            }
        }

        // ✅ NEW: Trigger passive reactions from the attacker
        if (this is Player playerAttacker)
        {
            foreach (var itemData in playerAttacker.equippedPassiveItems)
            {
                if (itemData.itemPrefab == null) continue;

                var reaction = itemData.itemPrefab.GetComponent<IDamageReaction>();
                if (reaction != null)
                    reaction.OnAfterDamage(playerAttacker, damageTarget, thisHitDamage);
            }
        }

        Debug.Log($"{characterName} Apply Hit #{currentHitIndex + 1}: {portion * 100f}% → {thisHitDamage} damage to {damageTarget.characterName}");

        if (hitData.vfxPrefab != null)
        {
            GameObject vfx = Instantiate(hitData.vfxPrefab, damageTarget.transform.position + hitData.vfxOffset, Quaternion.identity);
            Destroy(vfx, hitData.lifeTime);
        }

        if (hitData.sfxClip != null)
        {
            AudioSource.PlayClipAtPoint(hitData.sfxClip, damageTarget.transform.position);
        }

        currentHitIndex++;
    }


    #region StatusEffect

    public void ApplyStatusEffect(TurnStatusEffect newEffect)
    {
        // 🔒 Check for immunity from passives
        foreach (var blocker in GetComponentsInChildren<IStatusEffectListener>())
        {
            if (blocker.ShouldBlockStatus(this, newEffect))
            {
                Debug.Log($"🛡️ {characterName} blocked {newEffect.effectType} due to passive immunity.");
                return;
            }
        }

        // ✅ Continue with merge or add
        var existing = activeStatusEffects.Find(e => e.effectType == newEffect.effectType);
        if (existing != null)
        {
            existing.remainingTurns = Mathf.Max(existing.remainingTurns, newEffect.remainingTurns);
            existing.magnitude = Mathf.Max(existing.magnitude, newEffect.magnitude);
        }
        else
        {
            activeStatusEffects.Add(newEffect);
        }

        Debug.Log($"{characterName} gains {newEffect.effectType} for {newEffect.remainingTurns} turns.");
    }

    public virtual void ProcessStatusEffects()
    {

        for (int i = activeStatusEffects.Count - 1; i >= 0; i--)
        {
            TurnStatusEffect effect = activeStatusEffects[i];

            switch (effect.effectType)
            {
                case StatusEffectType.Stun:
                    Debug.Log($"{characterName} is stunned and will skip this turn.");
                    break;

                // 🔻 Debuffs
                case StatusEffectType.DefenseDown:
                    if (!effect.isApplied)
                    {
                        defense -= effect.magnitude;
                        effect.isApplied = true;
                        Debug.Log($"{characterName}'s defense is reduced by {effect.magnitude}.");
                    }
                    break;

                case StatusEffectType.AttackDown:
                    if (!effect.isApplied)
                    {
                        attack -= effect.magnitude;
                        effect.isApplied = true;
                        Debug.Log($"{characterName}'s attack is reduced by {effect.magnitude}.");
                    }
                    break;

                case StatusEffectType.FocusDown:
                    if (!effect.isApplied)
                    {
                        focus -= effect.magnitude;
                        effect.isApplied = true;
                        Debug.Log($"{characterName}'s focus is reduced by {effect.magnitude}.");
                    }
                    break;

                // 🔺 Buffs
                case StatusEffectType.AttackUp:
                    if (!effect.isApplied)
                    {
                        attack += effect.magnitude;
                        effect.isApplied = true;
                        Debug.Log($"{characterName}'s attack is increased by {effect.magnitude}.");
                    }
                    break;

                case StatusEffectType.DefenseUp:
                    if (!effect.isApplied)
                    {
                        defense += effect.magnitude;
                        effect.isApplied = true;
                        Debug.Log($"{characterName}'s defense is increased by {effect.magnitude}.");
                    }
                    break;

                case StatusEffectType.SpeedUp:
                    if (!effect.isApplied)
                    {
                        speed += effect.magnitude;
                        effect.isApplied = true;
                        Debug.Log($"{characterName}'s speed is increased by {effect.magnitude}.");
                    }
                    break;

                // 🧪 Damage-over-time effects
                case StatusEffectType.Radiation:
                    {
                        int radDamage = Mathf.RoundToInt(maxHP * 0.04f);
                        Debug.Log($"{characterName} suffers RADIATION for {radDamage} damage.");
                        TakeDamage(radDamage);
                        TryApplyHealReductionDebuff(effect.source);
                        break;
                    }

                case StatusEffectType.Bleed:
                    {
                        int bleedDamage = Mathf.RoundToInt(maxHP * 0.05f);
                        bool spreadToAllParts = false;

                        Character source = effect.source;

                        if (source != null)
                        {
                            foreach (var itemData in (source as Player)?.equippedPassiveItems ?? new List<PassiveItemData>())
                            {
                                if (itemData.itemPrefab == null) continue;
                                if (itemData.itemPrefab.GetComponent<BloodTuner>() != null)
                                {
                                    spreadToAllParts = true;
                                    break;
                                }
                            }
                        }

                        if (spreadToAllParts)
                        {
                            Debug.Log($"{characterName} suffers BLEED on ALL parts for {bleedDamage} damage.");
                            foreach (var part in bodyParts)
                            {
                                if (!part.IsDestroyed)
                                    part.ApplyDamage(bleedDamage, false);
                            }
                        }
                        else
                        {
                            Debug.Log($"{characterName} suffers BLEED for {bleedDamage} damage.");
                            TakeDamage(bleedDamage);

                            // 🔔 Notify observers (e.g., Blood Rush Core)
                            if (source != null)
                            {
                                foreach (var observer in source.GetComponentsInChildren<IStatusEffectListener>())
                                {
                                    observer.OnBleedDamageDealt(this, bleedDamage, source);
                                }
                            }
                        }

                        break;
                    }

                case StatusEffectType.Poison:
                    {
                        int poisonDamage = Mathf.RoundToInt(maxHP * 0.03f);
                        Debug.Log($"{characterName} suffers POISON for {poisonDamage} damage.");
                        TakeDamage(poisonDamage);
                        TryApplyNerveRotVialDebuff(effect.source);
                        break;
                    }
            }

            // ⏳ Countdown
            effect.remainingTurns--;

            // 🧹 Expire effect
            if (effect.remainingTurns <= 0)
            {
                switch (effect.effectType)
                {
                    // Reverse debuffs
                    case StatusEffectType.DefenseDown:
                        defense += effect.magnitude;
                        break;
                    case StatusEffectType.AttackDown:
                        attack += effect.magnitude;
                        break;
                    case StatusEffectType.FocusDown:
                        focus += effect.magnitude;
                        break;

                    // Reverse buffs
                    case StatusEffectType.AttackUp:
                        attack -= effect.magnitude;
                        break;
                    case StatusEffectType.DefenseUp:
                        defense -= effect.magnitude;
                        break;
                    case StatusEffectType.SpeedUp:
                        speed -= effect.magnitude;
                        break;
                }

                Debug.Log($"{characterName} is no longer affected by {effect.effectType}.");
                activeStatusEffects.RemoveAt(i);
            }
        }
    }

    private void TryApplyNerveRotVialDebuff(Character source)
    {
        if (source == null || !(source is Player playerSource)) return;

        foreach (var itemData in playerSource.equippedPassiveItems)
        {
            if (itemData.itemPrefab == null) continue;

            if (itemData.itemPrefab.GetComponent<NerveRotVials>() != null)
            {
                // ✅ Limit to max 3 stacks
                int currentStacks = activeStatusEffects
                    .Count(e => e.effectType == StatusEffectType.DefenseDown && e.source == source);

                if (currentStacks >= 3)
                {
                    Debug.Log($"{characterName} already has max Nerve Rot stacks.");
                    return;
                }

                TurnStatusEffect debuff = new TurnStatusEffect(
                    StatusEffectType.DefenseDown,
                    2, // turns
                    2  // magnitude
                );
                debuff.source = source;
                ApplyStatusEffect(debuff);

                Debug.Log($"{characterName} suffers -2 Defense from Nerve Rot Vials.");
                return;
            }
        }
    }

    public bool TryGetCustomInt(string key, out int value)
    {
        return customIntData.TryGetValue(key, out value);
    }

    public void SetCustomInt(string key, int value)
    {
        customIntData[key] = value;
    }

    private void TryApplyHealReductionDebuff(Character source)
    {
        if (source == null || !(source is Player playerSource)) return;

        foreach (var itemData in playerSource.equippedPassiveItems)
        {
            if (itemData.itemPrefab == null) continue;

            if (itemData.itemPrefab.GetComponent<IrradiationMatrix>() != null)
            {
                // Prevent duplicates
                if (HasStatusEffect(StatusEffectType.HealReduction)) return;

                TurnStatusEffect debuff = new TurnStatusEffect(
                    StatusEffectType.HealReduction,
                    3, // duration in turns
                    50 // 50% healing reduction
                );
                debuff.source = source;

                ApplyStatusEffect(debuff);
                Debug.Log($"{characterName} is afflicted with HEAL REDUCTION (-50% healing) for 3 turns.");
                return;
            }
        }
    }

    #endregion

    public bool HasStatusEffect(StatusEffectType type)
    {
        return activeStatusEffects.Exists(effect => effect.effectType == type && effect.remainingTurns > 0);
    }

    #region  Equipment

    public bool EquipWeapon(Weapon weapon, bool isRightHand)
    {
        if (weapon == null) return false;

        if (weapon.isTwoHandWeapon)
        {
            // Two-handed weapons occupy both hands
            leftHandWeapon = weapon;
            rightHandWeapon = weapon;
        }
        else
        {
            // Equip normally
            if (isRightHand)
                rightHandWeapon = weapon;
            else
                leftHandWeapon = weapon;

            // Unequip two-hand if one-handed weapon is equipped
            if (leftHandWeapon != rightHandWeapon)
            {
                if (leftHandWeapon?.isTwoHandWeapon == true)
                    leftHandWeapon = null;

                if (rightHandWeapon?.isTwoHandWeapon == true)
                    rightHandWeapon = null;
            }
        }

        return true;
    }

    public void UnequipRightHand()
    {
        rightHandWeapon = null;

        // If it's a two-handed weapon, remove both
        if (leftHandWeapon?.isTwoHandWeapon == true)
            leftHandWeapon = null;
    }

    public void UnequipLeftHand()
    {
        leftHandWeapon = null;

        // If it's a two-handed weapon, remove both
        if (rightHandWeapon?.isTwoHandWeapon == true)
            rightHandWeapon = null;
    }

    #endregion

    public void EquipAllPassives()
    {
        runtimePassiveBehaviors.Clear(); // Reset to avoid duplicates

        foreach (var item in equippedPassiveItems)
        {
            item.EquipTo(this); // this handles tracking internally
        }

        foreach (var part in bodyParts)
        {
            part.EquipArmorTo(this); // same idea
        }
    }

    public void RegisterRuntimePassive(GameObject instance)
    {
        foreach (var comp in instance.GetComponents<MonoBehaviour>())
        {
            runtimePassiveBehaviors.Add(comp);
            if (comp is IPassiveItem item) item.ApplyEffect(this);
        }
    }

    public string GetStatusChances()
    {
        Dictionary<StatusChanceType, float> allChances = possibilityPool.GetAllChances();
        List<string> lines = new();

        foreach (var kvp in allChances)
        {
            lines.Add($"{kvp.Key}: {Mathf.RoundToInt(kvp.Value * 100f)}%");
        }

        return string.Join(", ", lines);
    }



}
