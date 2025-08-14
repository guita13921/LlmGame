using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

[Serializable]
public class BodyPartOverride
{
    public BodyPartType type;
    public BodyPartState defaultState = BodyPartState.Intact;
    public BodyPartComposition composition = BodyPartComposition.Human;
    public List<string> keywords = new List<string>();
    public bool isVital = false;
    public int health = 100;
    public int maxHealth = 100;
    [Range(0f, 1f)] public float damageToPartRatio = 0.5f;

    [Header("LLM Influence")]
    [Range(-10f, 10f)] public float feasibilityModifier = 0f;
    [Range(-10f, 10f)] public float potentialModifier = 0f;


    public ArmorData defaultArmor;
    public ArmorData equippedArmor; // <- renamed from defaultArmor
    public WeakPointData weakPoint;

    [Tooltip("Optional descriptions for buffs/debuffs (e.g. 'Harder to hit', '-1 feasibility')")]
    public List<string> descriptions = new List<string>();
}
