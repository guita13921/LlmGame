using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Animation")]
    public Animator animator;
    [SerializeField] public bool animationFinished = false;
    BattleManager battleManager;

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

    [Header("Skills")]
    public List<DamageModifierSkill> damageModifierSkills = new List<DamageModifierSkill>();
    public DamageModifierSkill currentSkill;
    public bool isUsingUltimateSkill = false;

    [Header("Runtime")]
    [SerializeField] public int currentHP;
    [SerializeField] public int currentMP;
    [SerializeField] public int currentshield;
    public float turnGauge = 0f;

    [Header("Inventory")]
    public List<Item> inventoryItems;

    [Header("Equipment")]
    public Weapon leftHandWeapon;
    public Weapon rightHandWeapon;

    [Header("Active Items")]
    public List<Item> activeItem;
    public bool isUsingConsumeTurnItem;


    public virtual void Awake()
    {
        battleManager = FindAnyObjectByType<BattleManager>();
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


        // 🔥 BEFORE damage: check equipped PassiveItemData if damageTarget is Player
        if (damageTarget is Player playerTarget)
        {
            foreach (var itemData in playerTarget.equippedPassiveItems)
            {
                if (itemData.itemPrefab == null) continue;

                IDamageReaction reaction = itemData.itemPrefab.GetComponent<IDamageReaction>();
                if (reaction != null)
                {
                    reaction.OnBeforeDamage(this, playerTarget, ref thisHitDamage);
                }
            }
        }

        // 💥 Apply damage
        damageTarget.TakeDamage(thisHitDamage);


        // 💡 AFTER damage: prefab passive effects from equipped items
        if (damageTarget is Player playerTargetAfter)
        {
            foreach (var itemData in playerTargetAfter.equippedPassiveItems)
            {
                if (itemData.itemPrefab == null) continue;

                IDamageReaction reaction = itemData.itemPrefab.GetComponent<IDamageReaction>();
                if (reaction != null)
                {
                    reaction.OnAfterDamage(this, playerTargetAfter, thisHitDamage);
                }
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


    public void ApplyStatusEffect(TurnStatusEffect newEffect)
    {
        // Optional: merge with existing effect
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

            // Apply effect logic before reducing turn count
            switch (effect.effectType)
            {
                case StatusEffectType.Stun:
                    Debug.Log($"{characterName} is stunned and will skip this turn.");
                    break;

                case StatusEffectType.Flame:
                    int burnDamage = Mathf.RoundToInt(maxHP * 0.05f); // 5% of max HP
                    TakeDamage(burnDamage);
                    Debug.Log($"{characterName} takes {burnDamage} burn damage from Flame.");
                    break;

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

                    // Add more effects here
            }

            // Decrement turns
            effect.remainingTurns--;

            // Remove and revert stat effects if expired
            if (effect.remainingTurns <= 0)
            {
                switch (effect.effectType)
                {
                    case StatusEffectType.DefenseDown:
                        defense += effect.magnitude;
                        break;
                    case StatusEffectType.AttackDown:
                        attack += effect.magnitude;
                        break;
                    case StatusEffectType.FocusDown:
                        focus += effect.magnitude;
                        break;
                }

                Debug.Log($"{characterName} is no longer affected by {effect.effectType}.");
                activeStatusEffects.RemoveAt(i);
            }
        }
    }

    public bool HasStatusEffect(StatusEffectType type)
    {
        return activeStatusEffects.Exists(effect => effect.effectType == type && effect.remainingTurns > 0);
    }

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

}
