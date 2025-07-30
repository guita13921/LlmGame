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
}
