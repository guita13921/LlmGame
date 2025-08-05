using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewArmor", menuName = "Character/Armor")]
public class ArmorData : ScriptableObject
{
    [Header("General Armor Stats")]
    public string armorName;
    public float reducePotentialDamage;
    public float reduceFeasibility;
    public string description;

    [Header("DamageType Reduction")]
    public int reduceDamagePhysical;
    public int reduceDamageFire;
    public int reduceDamageElectric;
    public int reduceDamageRadiation;
    public int reduceDamageExplosive;
    public int reduceDamageDigital;
    public int reduceDamagePlasma;
    public int reduceDamageLaser;
    public int reduceDamageChemical;
    public int reduceDamageViral;
    public List<DamageType> damageTypeReduce;

}
