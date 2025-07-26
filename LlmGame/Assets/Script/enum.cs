using UnityEngine;

public enum ItemType
{
    Main_Weapon,
    Sub_Weapon,
    Other
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
    Digital,        // Direct neural interface attacks (hacking a cyberbrain)
    Plasma,         // High-energy plasma weapons
    Laser,          // Laser rifles, cutting beams
    Chemical,       // Gas attacks, chemical bombs
    Viral,          // Digital viruses that affect both tech and biology
}

public enum CharacterType
{
    Cyborg,
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
    LeftEye,
    RightEye,
    Heart
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
    Shock,
    Flame,
    DefenseDown,
    AttackDown,
    FocusDown,
    Custom // Add more as needed
}