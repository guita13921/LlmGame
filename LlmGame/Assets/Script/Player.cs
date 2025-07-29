using System;
using UnityEngine;

public class Player : Character
{
    [Header("Player Info")]
    public string classType;

    [Header("Player Stat")]
    public int bodyLimit;


    void Start()
    {
        //ApplyStatusEffect(new TurnStatusEffect(StatusEffectType.Stun, 1));
    }
}
