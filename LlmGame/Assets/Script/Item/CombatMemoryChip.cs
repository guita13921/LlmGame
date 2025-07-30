using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatMemoryChip : PassiveItemBase
{
    public override void ApplyEffect(Character character)
    {
        int bonus = (character.focus / 3) * 2;
        character.attack += bonus;
    }
}

