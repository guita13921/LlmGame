using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Weapon")]
public class Weapon : Item
{
    [Header("Stat")]
    public int Damage;

    [Header("DamageType")]
    public List<DamageType> damageType;
}
