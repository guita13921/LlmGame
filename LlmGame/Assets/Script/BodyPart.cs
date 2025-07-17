using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewBodyPart", menuName = "Character/Body Part")]
public class BodyPartData : ScriptableObject
{
    public BodyPartType type;
    public BodyPartState state = BodyPartState.Intact;
    public BodyPartComposition composition = BodyPartComposition.Human;
    public bool isVital = false;
    public int health = 100;

    [Header("Weak Point (Optional)")]
    public WeakPointData linkedWeakPoint;
}
