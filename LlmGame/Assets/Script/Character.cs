using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Basic Info")]
    public string characterName;
    public GameObject sprite;
    [TextArea] public string description;

    // Reference to BattleManager so we can inform it
    private BattleManager battleManager;

    [Header("Stats")]
    public int attack;
    public int defense;
    public int focus;
    public int maxHP;
    public int maxMP;
    public int speed;

    [Header("Runtime")]
    public int currentHP;
    public int currentMP;
    public float turnGauge = 0f;

    [Header("Inventory")]
    public List<Item> inventoryItems;

    [Header("Active Items")]
    public List<Item> activeItem;

    public virtual void Awake()
    {
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
            if (this is Enemy && this.IsAlive())
            {
                battleManager.PlayerSelectedTarget(this as Enemy);
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



}
