using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public override void Awake()
    {
        base.Awake();

        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.LoadOrInitPlayer(this);
        }
    }

    void Start()
    {
        EquipAllPassives();
        Debug.Log(GetStatusChances());
        //this.ApplyStatusEffect(new TurnStatusEffect(StatusEffectType.Stun, 2, 0));
        //this.ApplyStatusEffect(new TurnStatusEffect(StatusEffectType.Bleed, 2, 1));
        //this.ApplyStatusEffect(new TurnStatusEffect(StatusEffectType.Poison, 2, 1));
    }

    private void OnDestroy()
    {
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.SavePlayer(this);
        }
    }

}