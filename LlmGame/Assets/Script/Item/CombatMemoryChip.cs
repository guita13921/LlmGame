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
    }

    public override void DeApplyEffect(Character character)
    {
        character.attack -= bonus;
        bonus = 0;
    }
}

