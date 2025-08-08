using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    void Start()
    {
        EquipAllPassives();
        Debug.Log(GetStatusChances());
        //this.ApplyStatusEffect(new TurnStatusEffect(StatusEffectType.Stun, 2, 0));
        this.ApplyStatusEffect(new TurnStatusEffect(StatusEffectType.Bleed, 2, 1));
    }

}