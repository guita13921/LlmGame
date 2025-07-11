using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/DefensiveItem")]
public class Defensive : Item
{
    [Header("Stat")]
    public int reduceDamagePhysical;       // Conventional bullets; blades; blunt weapons
    public int reduceDamageFire;           // Flamethrowers; incendiary rounds; plasma cutters
    public int reduceDamageElectric;       // Shock batons; taser darts; EMP grenades
    public int reduceDamageRadiation;      // Dirty energy weapons; nuclear micro-explosives
    public int reduceDamageExplosive;      // Grenades; rocket launchers
    public int reduceDamageDigital;        // Direct neural interface attacks (hacking a cyberbrain)
    public int reduceDamagePlasma;         // High-energy plasma weapons
    public int reduceDamageLaser;          // Laser rifles; cutting beams
    public int reduceDamageChemical;       // Gas attacks; chemical bombs
    public int reduceDamageViral;          // Digital viruses that affect both tech and biology


    [Header("DamageType")]
    public List<DamageType> damageTypeReduce;
}