using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeurospikeBooster : PassiveItemBase
{
    public int speedBoost = 5;

    public override void ApplyEffect(Character character)
    {
        character.speed += speedBoost;
    }
}
