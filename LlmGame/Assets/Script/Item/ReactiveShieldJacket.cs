using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactiveShieldJacket : PassiveItemBase
{
    public override void ApplyEffect(Character character)
    {
        character.maxShield += 25;
        character.currentshield += 25;
    }

    public override void DeApplyEffect(Character character)
    {
        character.maxShield -= 25;
        character.currentshield = Mathf.Clamp(character.currentshield - 25, 0, character.maxShield);
    }
}
