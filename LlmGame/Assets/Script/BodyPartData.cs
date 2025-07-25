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
    public bool isVital = false;
    public int health = 100;
    public int maxHealth = 100;

    [Header("Weak Point (Optional)")]
    public WeakPointData linkedWeakPoint;

    [Header("Armor (Optional)")]
    public ArmorData equippedArmor;

    [Header("LLM Influence")]
    [Range(-10f, 10f)] public float feasibilityModifier = 0f;
    [Range(-10f, 10f)] public float potentialModifier = 0f;

    [Header("Damage Control")]
    [Range(0f, 1f)] public float damageToPartRatio = 0.5f;
    public bool becomesWeakPointWhenDestroyed = true;


    public bool IsDestroyed => health <= 0;

    public void ApplyDamage(int totalDamage)
    {
        int damageToPart = Mathf.RoundToInt(totalDamage * damageToPartRatio);

        int beforeHealth = this.health;
        this.health -= damageToPart;

        if (health < 0) health = 0;

        // 🧠 Debug log
        Debug.Log($"💥 [Damage{totalDamage}] {type} took {damageToPart} damage. HP: {beforeHealth} → {health}");

        // Check for destruction
        if (IsDestroyed && becomesWeakPointWhenDestroyed)
        {
            if (linkedWeakPoint != null)
            {
                linkedWeakPoint.isExposed = true;
                Debug.Log($"⚠️ [BodyPart] {type} destroyed — weak point '{linkedWeakPoint.weakPointName}' is now exposed.");
            }
        }
    }

}
