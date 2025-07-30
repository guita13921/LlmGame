using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageReaction
{
    void OnBeforeDamage(Character source, Character target, ref int damage);
    void OnAfterDamage(Character source, Character target, int finalDamage);
}