using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MSGNoodle : PassiveItemBase
{
    public override void ApplyEffect(Character character)
    {
        character.maxHP += 10;
        character.currentHP += 10;
    }
}
