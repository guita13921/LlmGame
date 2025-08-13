using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitaniumLatticeArmor : PassiveItemBase
{
    public override void ApplyEffect(Character character)
    {
        character.defense += 5;
        character.bonusDefense += 5;
    }

    public override void DeApplyEffect(Character character)
    {
        character.defense -= 5;
        character.bonusDefense -= 5;
    }
}
