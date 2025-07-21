using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumeTurnItem : Item
{
    public virtual IEnumerator UseOnTarget(Character user, Character target, BattleManager battleManager)
    {
        yield return null; // base does nothing
    }
}