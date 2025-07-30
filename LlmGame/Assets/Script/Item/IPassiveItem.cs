using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPassiveItem
{
    void ApplyEffect(Character character);
    public void OnAfterDamage(Character source, Character target, int finalDamage);
    public void OnBeforeDamage(Character source, Character target, ref int damage);
}