using System.Diagnostics;

public class Player : Character
{
    public string ClassType;
    public int CurrentEXP;
    public int Level;
    public int StatPoints;
    private int baseEXP = 100;
    private double growth = 1.5;
    public int MaxLevel = 10;

    public Player(string classType, string name, string description, int attack, int defense, int focus, int maxHP, int maxMP, int speed)
        : base(name, description, attack, defense, focus, maxHP, maxMP, speed)
    {
        ClassType = classType;
        Level = 1;
        CurrentEXP = 0;
        StatPoints = 0;
    }

    public int EXPToNextLevel()
    {
        return (int)(baseEXP * System.Math.Pow(Level, growth));
    }

    public void GainEXP(int amount)
    {
        if (Level >= MaxLevel)
        {
            Debug.WriteLine($"{Name} is already at max level!");
            return;
        }

        CurrentEXP += amount;
        Debug.WriteLine($"{Name} gained {amount} EXP! (Current EXP: {CurrentEXP}/{EXPToNextLevel()})");

        while (CurrentEXP >= EXPToNextLevel() && Level < MaxLevel)
        {
            CurrentEXP -= EXPToNextLevel();
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Level++;
        StatPoints += 3; // เช่น ได้ 3 แต้มต่อเลเวลอัป
        Debug.WriteLine($"{Name} leveled up to LV.{Level}! You gained 3 stat points.");
    }

    public void AllocateStat(string stat, int points)
    {
        if (points > StatPoints)
        {
            Debug.WriteLine("Not enough stat points!");
            return;
        }

        switch (stat.ToLower())
        {
            case "hp":
                MaxHP += points * 10;
                CurrentHP += points * 10; // เพิ่ม HP ปัจจุบันด้วย
                break;

            case "mp":
                MaxMP += points * 10;
                CurrentMP += points * 10;
                break;

            case "speed":
                Speed += points;
                break;

            case "focus":
                if (points % 2 != 0)
                {
                    Debug.WriteLine("Focus: ต้องใช้เป็นจำนวนคู่ (2 แต้มต่อ +1 Focus)");
                    return;
                }
                Focus += points / 2;
                break;

            default:
                Debug.WriteLine("Unknown stat name!");
                return;
        }

        StatPoints -= points;
        Debug.WriteLine($"{points} point(s) allocated to {stat}.");
    }
}
