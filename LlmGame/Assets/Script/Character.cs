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
    public GameObject sprite;
    [TextArea] public string description;

    [Header("Stats")]
    public int attack;
    public int defense;
    public int focus;
    public int maxHP;
    public int maxMP;
    public int speed;

    [Header("Actions")]
    public List<CharacterActionData> availableActions = new List<CharacterActionData>();
    public CharacterActionData selectedAction;
    public int pendingDamage;
    public List<float> damagePortions = new List<float>();
    public Character damageTarget;
    public int currentHitIndex = 0;

    [Header("Runtime")]
    public int currentHP;
    public int currentMP;
    public float turnGauge = 0f;

    [Header("Inventory")]
    public List<Item> inventoryItems;

    [Header("Active Items")]
    public List<Item> activeItem;
    public bool isUsingConsumeTurnItem;

    public virtual void Awake()
    {
        battleManager = FindAnyObjectByType<BattleManager>();
        animator = GetComponent<Animator>();
        currentHP = maxHP;
        currentMP = maxMP;
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
            Debug.LogWarning($"damageTarget");
            return;
        }

        if (currentHitIndex >= damagePortions.Count)
        {
            Debug.LogWarning($"{characterName} has no more damagePortions left.");
            return;
        }

        float portion = damagePortions[currentHitIndex];
        int thisHitDamage = Mathf.RoundToInt(pendingDamage * portion);
        damageTarget.TakeDamage(thisHitDamage);

        Debug.Log($"{characterName} Apply Hit #{currentHitIndex + 1}: {portion * 100f}% → {thisHitDamage} damage to {damageTarget.characterName}");

        currentHitIndex++;
    }


}
