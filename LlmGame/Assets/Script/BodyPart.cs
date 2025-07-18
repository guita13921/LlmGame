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
    public List<string> keyword;
    public bool isVital;
    public int health = 100;

    [Header("Damage Control")]
    [Range(0f, 1f)] public float damageToPartRatio = 0.5f;
    public bool becomesWeakPointWhenDestroyed = true;

    [Header("Weak Point (Optional)")]
    public WeakPointData linkedWeakPoint;

    [Header("Armor (Optional)")]
    public ArmorData equippedArmor;

    public bool IsDestroyed => health <= 0;

    public void ApplyDamage(int totalDamage)
    {
        int damageToPart = Mathf.RoundToInt(totalDamage * damageToPartRatio);
        health -= damageToPart;
        if (health < 0) health = 0;

        if (IsDestroyed && becomesWeakPointWhenDestroyed)
        {
            state = BodyPartState.Missing;

            if (linkedWeakPoint == null)
            {
                linkedWeakPoint = ScriptableObject.CreateInstance<WeakPointData>();
                linkedWeakPoint.weakPointName = $"{type} Core";
                linkedWeakPoint.weakPointDescription = $"Weak point revealed after destruction of {type}.";
                linkedWeakPoint.isExposed = true;
                linkedWeakPoint.hideFlags = HideFlags.DontSave;
            }
            else
            {
                linkedWeakPoint.isExposed = true;
            }
        }
    }
}
