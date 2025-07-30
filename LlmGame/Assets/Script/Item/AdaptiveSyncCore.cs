using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptiveSyncCore : PassiveItemBase
{
    public override void ApplyEffect(Character character)
    {
        character.attack += 5;
        character.defense += 5;
        character.focus += 5;
        character.maxHP += 5;
        character.currentHP += 5;
        character.maxMP += 5;
        character.currentMP += 5;
        character.speed += 5;
    }
}
