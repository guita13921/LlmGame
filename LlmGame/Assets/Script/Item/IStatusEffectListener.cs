using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IStatusEffectListener
{
    bool ShouldSpreadBleed(Character character);

    /// Called before a status effect is applied. Return true to cancel/block it.
    bool ShouldBlockStatus(Character character, TurnStatusEffect effect);

    void OnBleedDamageDealt(Character target, int damage, Character source);
}
