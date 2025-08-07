using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class ToxicVisor : MonoBehaviour, IPassiveItem
{
    public void ApplyEffect(Character character)
    {
        // No immediate effect on equip
    }

    public void OnAfterDamage(Character source, Character target, int finalDamage) { }

    public void OnBeforeDamage(Character source, Character target, ref int damage) { }
}
