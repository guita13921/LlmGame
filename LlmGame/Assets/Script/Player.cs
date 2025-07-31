using System;
using System.Collections.Generic;
using UnityEngine;

public class Player : Character
{

    void Start()
    {
        EquipPassiveItems();
        Debug.Log(GetStatusChances());
    }

}