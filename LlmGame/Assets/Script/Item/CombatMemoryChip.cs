using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatMemoryChip : PassiveItemBase
{
    private int bonus = 0;
    public override void ApplyEffect(Character character)
    {
        bonus = (character.focus / 3) * 2;
        character.attack += bonus;
        character.bonusAttack += bonus;
    }

    public override void DeApplyEffect(Character character)
    {
        character.attack -= bonus;
        character.bonusAttack -= bonus;
        bonus = 0;
    }
}

