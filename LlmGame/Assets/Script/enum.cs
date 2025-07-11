using UnityEngine;

public enum ItemType
{
    Offensive,
    Defensive,
    Healing
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
