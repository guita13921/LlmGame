using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Weapon")]
public class Weapon : Item
{
    [Header("Stat")]
    public int damagePhysical;       // Conventional bullets; blades; blunt weapons
    public int damageFire;           // Flamethrowers; incendiary rounds; plasma cutters
    public int damageElectric;       // Shock batons; taser darts; EMP grenades
    public int damageRadiation;      // Dirty energy weapons; nuclear micro-explosives
    public int damageExplosive;      // Grenades; rocket launchers
    public int damageDigital;        // Direct neural interface attacks (hacking a cyberbrain)
    public int damagePlasma;         // High-energy plasma weapons
    public int damageLaser;          // Laser rifles; cutting beams
    public int damageChemical;       // Gas attacks; chemical bombs
    public int damageViral;          // Digital viruses that affect both tech and biology

    [Header("DamageType")]
    public List<DamageType> damageType;
}
