using UnityEngine;

public class Character : MonoBehaviour
{
    [Header("Basic Info")]
    public string characterName;
    public Sprite sprite;
    [TextArea] public string description;

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


    public virtual void Awake()
    {
        currentHP = maxHP;
        currentMP = maxMP;
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
