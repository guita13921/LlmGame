using System;
using UnityEngine;

public class Player : Character
{
    [Header("Player Info")]
    public string classType;
    public int level = 1;
    public int currentEXP = 0;
    public int statPoints = 0;

    [Header("Player Inventory")]
    public string itemName;
    [TextArea] public string itemDescription;


    public int maxLevel = 10;
    private int baseEXP = 100;
    private double growth = 1.5;

    public int EXPToNextLevel()
    {
        return (int)(baseEXP * Mathf.Pow(level, (float)growth));
    }

    public void GainEXP(int amount)
    {
        if (level >= maxLevel)
        {
            Debug.Log($"{characterName} is already at max level!");
            return;
        }

        currentEXP += amount;
        Debug.Log($"{characterName} gained {amount} EXP! (Current EXP: {currentEXP}/{EXPToNextLevel()})");

        while (currentEXP >= EXPToNextLevel() && level < maxLevel)
        {
            currentEXP -= EXPToNextLevel();
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        statPoints += 3;
        Debug.Log($"{characterName} leveled up to LV.{level}! You gained 3 stat points.");
    }

    public void AllocateStat(string stat, int points)
    {
        if (points > statPoints)
        {
            Debug.Log("Not enough stat points!");
            return;
        }

        switch (stat.ToLower())
        {
            case "hp":
                maxHP += points * 10;
                currentHP += points * 10;
                break;
            case "mp":
                maxMP += points * 10;
                currentMP += points * 10;
                break;
            case "speed":
                speed += points;
                break;
            case "focus":
                if (points % 2 != 0)
                {
                    Debug.Log("Focus: ต้องใช้เป็นจำนวนคู่ (2 แต้มต่อ +1 Focus)");
                    return;
                }
                focus += points / 2;
                break;
            default:
                Debug.Log("Unknown stat name!");
                return;
        }

        statPoints -= points;
        Debug.Log($"{points} point(s) allocated to {stat}.");
    }
}
