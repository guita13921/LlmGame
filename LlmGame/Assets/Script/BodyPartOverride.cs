using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BodyPartOverride
{
    public BodyPartType type;
    public bool isVital;
    public int health = 100;
    [Range(0f, 1f)] public float damageToPartRatio = 0.5f;
    public BodyPartState defaultState = BodyPartState.Intact;
    public BodyPartComposition composition = BodyPartComposition.Human;

    public List<string> keywords;

    [Header("Optional References")]
    public ArmorData defaultArmor;
    public WeakPointData weakPoint;
}
