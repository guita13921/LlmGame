using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterAction", menuName = "Battle/Character Action")]
public class CharacterActionData : ScriptableObject
{
    public string actionName;
    public string animationTrigger;

    [Tooltip("Each portion represents a % of final damage per hit. Example: [0.2, 0.3, 0.5] for 3 hits.")]
    public List<float> damagePortions = new List<float> { 1.0f };
}
