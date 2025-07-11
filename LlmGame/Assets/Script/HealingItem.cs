using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/HealingItem")]
public class HealingItem : Item
{
    [Header("Stat")]
    public int healingAmount;

    [Header("Hidden Stat")]
    public int Damage;

    [Header("DamageType")]
    public List<DamageType> damageType;
}
