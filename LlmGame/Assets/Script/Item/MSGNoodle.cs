using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MSGNoodle : PassiveItemBase
{
    public override void ApplyEffect(Character character)
    {
        character.maxHP += 10;
        character.bonusMaxHP += 10;
        character.currentHP += 10;
    }

    public override void DeApplyEffect(Character character)
    {
        character.maxHP -= 10;
        character.bonusMaxHP -= 10;
        character.currentHP = Mathf.Clamp(character.currentHP - 10, 0, character.maxHP);
    }
}
