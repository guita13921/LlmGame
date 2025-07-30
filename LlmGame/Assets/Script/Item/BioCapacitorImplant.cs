using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BioCapacitorImplant : PassiveItemBase
{
    public override void ApplyEffect(Character character)
    {
        character.maxMP += 15;
        character.currentMP += 15;
    }
}
