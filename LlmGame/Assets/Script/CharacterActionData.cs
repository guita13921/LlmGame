using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterAction", menuName = "Battle/Character Action")]
public class CharacterActionData : ScriptableObject
{
    public string actionName;
    public string animationTrigger;

    [Tooltip("Number of turns before this action resolves. 0 means immediate.")]
    public int delayTurns = 0;

    [Header("Multi-Hit Setup")]
    [Tooltip("List of effects and damage split for each hit.")]
    public List<HitEffectData> hitEffects = new List<HitEffectData> { new HitEffectData() };

    [Header("Status Effect Chances")]
    [Range(0f, 1f)] public float bleedChance = 0f;
    [Range(0f, 1f)] public float poisonChance = 0f;
    [Range(0f, 1f)] public float stunChance = 0f;
    [Range(0f, 1f)] public float criticalChance = 0f;
}
