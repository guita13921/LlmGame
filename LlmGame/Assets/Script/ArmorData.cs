using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewArmor", menuName = "Character/Armor")]
public class ArmorData : ScriptableObject
{
    [Header("General Armor Stats")]
    public string armorName;
    [TextArea]
    public string description;

    [Header("Slot Compatibility")]
    public List<BodyPartType> compatibleBodyParts; // ✅ NEW FIELD

    [Header("Stat Reductions")]
    public float reducePotentialDamage;
    public float reduceFeasibility;

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

    [Header("Item Logic (Passive/Active)")]
    public GameObject itemBehaviorPrefab;
}
