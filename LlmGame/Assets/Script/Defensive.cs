using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/DefensiveItem")]
public class Defensive : Item
{
    [Header("Stat")]
    public int reduceDamage;

    [Header("DamageType")]
    public List<DamageType> damageTypeReduce;
}