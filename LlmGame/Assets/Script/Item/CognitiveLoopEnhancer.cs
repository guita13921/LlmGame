using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CognitiveLoopEnhancer : PassiveItemBase
{
    public override void ApplyEffect(Character character)
    {
        character.focus += 5;
    }
}
