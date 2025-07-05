using System;

public class Character
{
    public string Name;
    public string Description;
    public int Attack;
    public int Defense;
    public int Focus;
    public int MaxHP;
    public int MaxMP;
    public int Speed;
    public int CurrentHP;
    public int CurrentMP;
    public string LLMprompt;
    public float TurnGauge = 0f;

    public Character(string name, string description, int attack, int defense, int focus, int maxHP, int maxMP, int speed)
    {
        Name = name;
        Description = description;
        Attack = attack;
        Defense = defense;
        Focus = focus;
        MaxHP = maxHP;
        MaxMP = maxMP;
        Speed = speed;

        CurrentHP = maxHP;
        CurrentMP = maxMP;
    }

    public virtual void TakeDamage(int dmg)
    {
        int finalDamage = Math.Max(dmg - Defense, 0);
        CurrentHP -= finalDamage;
        if (CurrentHP < 0) CurrentHP = 0;
        Console.WriteLine($"{Name} takes {finalDamage} damage! (HP left: {CurrentHP})");
    }

    public virtual bool IsAlive()
    {
        return CurrentHP > 0;
    }

    public string GetStatus()
    {
        return $"HP: {CurrentHP}/{MaxHP}, MP: {CurrentMP}/{MaxMP}";
    }
}
