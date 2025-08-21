using UnityEngine;

public enum ItemType
{
    Main_Weapon,
    Sub_Weapon,
    Other
}

public enum ItemRarity
{
    Common,
    Rare,
    Epic
}

public enum WeaponType
{
    Melee_Weapon,
    Ranged_Weapon
}

public enum UsageType
{
    OneTime,
    Infinite
}

public enum DamageType
{
    Physical,       // Conventional bullets, blades, blunt weapons
    Fire,           // Flamethrowers, incendiary rounds, plasma cutters
    Electric,       // Shock batons, taser darts, EMP grenades
    Radiation,      // Dirty energy weapons, nuclear micro-explosives
    Explosive,      // Grenades, rocket launchers
    Plasma,         // High-energy plasma weapons
    Poison,       // Gas attacks, chemical bombs
    Viral,          // Digital viruses that affect both tech and biology
}

public enum CharacterType
{
    Android,
    Human
}

public enum BodyPartType
{
    Head,
    Torso,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg,
}

public enum BodyPartState
{
    Intact,
    Damaged,
    Missing
}

public enum BodyPartComposition
{
    Human,
    Cybernetic,
    Robotic
}

public class StatusEffect
{
    public string effectName;
    public int turnsRemaining;
    public bool skipTurn;
}

public enum StatusEffectType
{
    Stun,
    DefenseDown,
    AttackDown,
    FocusDown,
    Bleed,
    Poison,
    Radiation,
    Contaminated,
    AttackUp,
    DefenseUp,
    SpeedUp,
    CritChanceUp,
    CritDamageUp,
    HealReduction
}

public enum StatusChanceType
{
    Bleed,
    Poison,
    Stun,
    Critical
}

public enum EnemyDifficulty
{
    Easy,
    Normal,
    Hard,
    None
}
