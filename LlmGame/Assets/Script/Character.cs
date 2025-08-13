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
    public int mpRegenPerTurn;
    public List<MonoBehaviour> runtimePassiveBehaviors = new();
    public float turnGauge = 0f;

    [Header("Inventory")]
    [SerializeField] public List<Item> inventoryItems;
    [SerializeField] public List<ArmorData> inventoryArmors;

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
        if (HasStatusEffect(StatusEffectType.Contaminated))
        {
            dmg = Mathf.RoundToInt(dmg * 1.25f);
        }
        int finalDamage = Mathf.Max(dmg - defense, 0);

        if (currentshield > 0)
        {
            int shieldDamage = Mathf.Min(currentshield, finalDamage);
            currentshield -= shieldDamage;
            finalDamage -= shieldDamage;
        }

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

        // 🔥 Log the attack outcome here
        LogAttackOutcome();
    }

    public void ApplyDamageAtHit()
    {
        if (damageTarget == null || !damageTarget.IsAlive()) return;

        if (selectedAction == null || selectedAction.hitEffects == null || currentHitIndex >= selectedAction.hitEffects.Count) return;

        var hitData = selectedAction.hitEffects[currentHitIndex];
        float portion = hitData.damagePortion;
        int thisHitDamage = Mathf.RoundToInt(pendingDamage * portion);

        damageTarget.TakeDamage(thisHitDamage);

        // VFX / SFX
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

    public void LogAttackOutcome()
    {
        if (damageTarget == null) return;

        string actionText = "";

        // For player, use the input field for flavor text (if exists)
        if (this is Player && battleManager != null && battleManager.playerInputField != null)
        {
            actionText = battleManager.playerInputField.text;
        }
        else if (selectedAction != null)
        {
            actionText = $"used {selectedAction.actionName}";
        }

        string log = $"Turn {battleManager.turnCount}: {characterName} {actionText} → Target: {damageTarget.characterName} Result: {damageTarget.currentHP} / {damageTarget.maxHP} ({battleManager.chatAI.baseEffect})";
        battleManager.battleLog.Add(log);

        //Debug.Log(log);
        //Debug.Log(damageTarget.GetBodyPartStatus());
    }


    #region StatusEffect

    public void ApplyStatusEffect(TurnStatusEffect newEffect)
    {
        Character source = newEffect.source;
        if (newEffect.effectType == StatusEffectType.Bleed && characterType == CharacterType.Android) return;
        if (newEffect.effectType == StatusEffectType.Poison && characterType == CharacterType.Android) return;

        if (source != null)
        {
            bool toxicVisorFound = false;

            // ✅ Check armor behaviors on source's body parts
            foreach (var bodyPart in source.bodyParts)
            {
                var armor = bodyPart.equippedArmor;
                if (armor == null || armor.itemBehaviorPrefab == null) continue;

                if (armor.itemBehaviorPrefab.GetComponent<ToxicVisor>() != null)
                {
                    toxicVisorFound = true;
                    break;
                }
            }

            // ✅ Apply Toxic Visor logic
            if (toxicVisorFound &&
                (newEffect.effectType == StatusEffectType.Poison || newEffect.effectType == StatusEffectType.Radiation))
            {
                newEffect.remainingTurns += 1;
            }
        }

        // Radiation reduces human attack
        if (newEffect.effectType == StatusEffectType.Radiation && characterType == CharacterType.Human)
        {
            int reduction = Mathf.RoundToInt(attack * 0.25f);
            TurnStatusEffect atkDown = new TurnStatusEffect(StatusEffectType.AttackDown, newEffect.remainingTurns, reduction, newEffect.source);
            ApplyStatusEffect(atkDown);
        }

        // ✅ Merge with existing effect if one already exists
        var existing = activeStatusEffects.Find(e => e.effectType == newEffect.effectType);
        if (existing != null)
        {
            existing.remainingTurns = existing.isPermanent ? int.MaxValue : Mathf.Max(existing.remainingTurns, newEffect.remainingTurns);
            if (newEffect.effectType == StatusEffectType.CritChanceUp || newEffect.effectType == StatusEffectType.CritDamageUp)
                existing.magnitude += newEffect.magnitude;
            else
                existing.magnitude = Mathf.Min(existing.magnitude + newEffect.magnitude, 3);
            existing.isPermanent = existing.isPermanent || newEffect.isPermanent;
        }
        else
        {
            activeStatusEffects.Add(newEffect);
            Debug.Log($"{characterName} gains {newEffect.effectType} for {newEffect.remainingTurns} turns.");
        }

        string durationText = newEffect.isPermanent ? "permanently" : $"{newEffect.remainingTurns} turns";
        Debug.Log($"{characterName} gains {newEffect.effectType} for {durationText}.");
        // Check for contamination
        var poison = activeStatusEffects.Find(e => e.effectType == StatusEffectType.Poison);
        var radiation = activeStatusEffects.Find(e => e.effectType == StatusEffectType.Radiation);
        if (poison != null && radiation != null && !HasStatusEffect(StatusEffectType.Contaminated))
        {
            int duration = Mathf.Min(poison.remainingTurns, radiation.remainingTurns);
            TurnStatusEffect contam = new TurnStatusEffect(StatusEffectType.Contaminated, duration, 0, newEffect.source);
            activeStatusEffects.Add(contam);
            Debug.Log($"{characterName} becomes CONTAMINATED.");
        }
    }


    public virtual void ProcessStatusEffects()
    {
        if (mpRegenPerTurn > 0)
        {
            int before = currentMP;
            currentMP = Mathf.Min(maxMP, currentMP + mpRegenPerTurn);
            int gained = currentMP - before;
            if (gained > 0)
                Debug.Log($"{characterName} regenerates {gained} MP.");
        }

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
                case StatusEffectType.CritChanceUp:
                    if (!effect.isApplied)
                    {
                        possibilityPool.AddModifier(StatusChanceType.Critical, effect.magnitude / 100f);
                        effect.isApplied = true;
                        Debug.Log($"{characterName}'s critical chance is increased by {effect.magnitude}%.");
                    }
                    break;
                case StatusEffectType.CritDamageUp:
                    if (!effect.isApplied)
                    {
                        possibilityPool.AddCriticalMultiplierBonus(effect.magnitude / 100f);
                        effect.isApplied = true;
                        Debug.Log($"{characterName}'s critical damage is increased by {effect.magnitude}%.");
                    }
                    break;

                // 🧪 Damage-over-time effects
                case StatusEffectType.Radiation:
                    {
                        float radPercent = 0.05f;
                        if (effect.magnitude == 2) radPercent = 0.10f;
                        else if (effect.magnitude >= 3) radPercent = 0.15f;
                        int radDamage = Mathf.RoundToInt(maxHP * radPercent);
                        if (characterType == CharacterType.Android)
                            radDamage *= 2;
                        Debug.Log($"{characterName} suffers RADIATION for {radDamage} damage.");
                        TakeDamage(radDamage);
                        TryApplyHealReductionDebuff(effect.source);
                        break;
                    }
                case StatusEffectType.Bleed:
                    {
                        float bleedPercent = 0.15f;
                        if (effect.magnitude == 2) bleedPercent = 0.20f;
                        else if (effect.magnitude >= 3) bleedPercent = 0.25f;
                        int bleedDamage = Mathf.RoundToInt(currentHP * bleedPercent);
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
                            if (currentshield <= 0)
                            {
                                foreach (var part in bodyParts)
                                {
                                    if (!part.IsDestroyed)
                                        part.ApplyDamage(bleedDamage, false);
                                }
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
                        float poisonPercent = 0.05f;
                        if (effect.magnitude == 2) poisonPercent = 0.10f;
                        else if (effect.magnitude >= 3) poisonPercent = 0.15f;
                        int poisonDamage = Mathf.RoundToInt(maxHP * poisonPercent);
                        Debug.Log($"{characterName} suffers POISON for {poisonDamage} damage.");
                        TakeDamage(poisonDamage);
                        TryApplyNerveRotVialDebuff(effect.source);
                        break;
                    }
            }

            // ⏳ Countdown
            if (!effect.isPermanent)
                effect.remainingTurns--;

            // 🧹 Expire effect
            if (!effect.isPermanent && effect.remainingTurns <= 0)
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
                    case StatusEffectType.CritChanceUp:
                        possibilityPool.AddModifier(StatusChanceType.Critical, -effect.magnitude / 100f);
                        break;
                    case StatusEffectType.CritDamageUp:
                        possibilityPool.AddCriticalMultiplierBonus(-effect.magnitude / 100f);
                        break;
                }

                Debug.Log($"{characterName} is no longer affected by {effect.effectType}.");
                activeStatusEffects.RemoveAt(i);
            }
        }
        var poison = activeStatusEffects.Find(e => e.effectType == StatusEffectType.Poison);
        var radiation = activeStatusEffects.Find(e => e.effectType == StatusEffectType.Radiation);
        var contam = activeStatusEffects.Find(e => e.effectType == StatusEffectType.Contaminated);
        if (poison != null && radiation != null)
        {
            int duration = Mathf.Min(poison.remainingTurns, radiation.remainingTurns);
            if (contam != null)
                contam.remainingTurns = duration;
            else
                activeStatusEffects.Add(new TurnStatusEffect(StatusEffectType.Contaminated, duration, 0));
        }
        else if (contam != null)
        {
            activeStatusEffects.Remove(contam);
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
        return activeStatusEffects.Exists(effect => effect.effectType == type && (effect.isPermanent || effect.remainingTurns > 0));
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
