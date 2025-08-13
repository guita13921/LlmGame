using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NanoVascularMicrorobot : PassiveItemBase
{
    public override void ApplyEffect(Character character)
    {
        Debug.Log("NanoVascularMicrorobot");
        character.maxHP += 25;
        character.bonusMaxHP += 25;
        character.currentHP += 25;
    }

    public override void DeApplyEffect(Character character)
    {
        character.maxHP -= 25;
        character.bonusMaxHP -= 25;
        character.currentHP = Mathf.Clamp(character.currentHP - 25, 0, character.maxHP);
    }
}
