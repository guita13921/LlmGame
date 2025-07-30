using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitaniumLatticeArmor : PassiveItemBase
{
    public override void ApplyEffect(Character character)
    {
        character.defense += 5;
    }
}
