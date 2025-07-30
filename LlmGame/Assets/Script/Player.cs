using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{
    public List<PassiveItemData> equippedPassiveItems;

    void Start()
    {
        EquipAllPassiveItems();
    }

    public void EquipAllPassiveItems()
    {
        foreach (var itemData in equippedPassiveItems)
        {
            itemData.EquipTo(this);
        }
    }

}