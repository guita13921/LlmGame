using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterAction", menuName = "Battle/Character Action")]
public class CharacterActionData : ScriptableObject
{
    public string actionName;
    public string animationTrigger;

    [Header("Multi-Hit Setup")]
    [Tooltip("List of effects and damage split for each hit.")]
    public List<HitEffectData> hitEffects = new List<HitEffectData> { new HitEffectData() };
}
