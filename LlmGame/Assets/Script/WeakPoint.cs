using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeakPoint", menuName = "Character/Weak Point")]
public class WeakPointData : ScriptableObject
{

    [Header("LLM Influence")]
    [Range(-10f, 10f)] public float income_feasibilityModifier = 0f;
    [Range(-10f, 10f)] public float income_potentialModifier = 0f;

    [Range(-10f, 10f)] public float outcome_feasibilityModifier = 0f;
    [Range(-10f, 10f)] public float outcome_potentialModifier = 0f;

    public string weakPointName;
    public bool isExposed = false;

    [TextArea]
    public string weakPointDescription;

    void Awake()
    {
        isExposed = false;
    }
}
