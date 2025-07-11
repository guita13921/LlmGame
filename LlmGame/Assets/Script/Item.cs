using System.Collections.Generic;
using UnityEngine;

public class Item : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;

    [TextArea]
    public string itemDescription;

    [Header("Usage")]
    public UsageType usageType;
    public int remain;

    [Header("KeyWord")]
    public List<string> keyWords;

    [Header("Activate")]
    public bool isActive;

    [Header("Consume Turn Item")]
    public bool isConsumeTurn;
}


