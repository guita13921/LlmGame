using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public void ApplyDamage(int totalDamage, bool hasReduce = true, Weapon weaponUsed = null)
    {
        int damageToPart = hasReduce
            ? Mathf.RoundToInt(totalDamage * damageToPartRatio)
            : Mathf.RoundToInt(totalDamage);

        int beforeHealth = health;
        health = Mathf.Max(0, health - damageToPart);

        Debug.Log($"💥 [Damage {totalDamage}] {type} took {damageToPart} damage. HP: {beforeHealth} → {health}");

        if (IsDestroyed && becomesWeakPointWhenDestroyed)
        {
            if (linkedWeakPoint != null)
            {
                Debug.Log($"⚠️ [BodyPart] {type} destroyed — weak point '{linkedWeakPoint.weakPointName}' is now exposed.");
            }

            if (weaponUsed != null && weaponUsed.weakPointType != null)
            {
                linkedWeakPoint = ScriptableObject.Instantiate(weaponUsed.weakPointType);
                Debug.Log($"🔄 Assigned new weak point from weapon: {linkedWeakPoint.weakPointName}");
            }
        }
    }

    public void EquipArmorTo(Character character, ArmorData armorToEquip)
    {
        if (armorToEquip == null || armorToEquip.itemBehaviorPrefab == null)
            return;

        GameObject instance = Instantiate(armorToEquip.itemBehaviorPrefab, character.transform);

        // Add the root component (or a specific script) to runtime list
        var behavior = instance.GetComponent<MonoBehaviour>();
        if (behavior != null)
            character.runtimePassiveBehaviors.Add(behavior);

        Debug.Log($"✅ Equipped armor with behavior: {armorToEquip.armorName}");
    }


    public bool TryEquipArmor(ArmorData armor)
    {
        if (armor == null)
            return false;

        if (!armor.compatibleBodyParts.Contains(type))
        {
            Debug.LogWarning($"❌ Cannot equip {armor.armorName} to {type}: incompatible slot.");
            return false;
        }

        equippedArmor = armor;
        Debug.Log($"✅ {armor.armorName} equipped to {type}.");
        return true;
    }

    public void ClearArmor(Character character)
    {
        if (equippedArmor == null)
            return;

        // Attempt to find and remove the runtime passive behavior if it exists
        if (equippedArmor.itemBehaviorPrefab != null)
        {
            string prefabName = equippedArmor.itemBehaviorPrefab.name;

            // Find the attached MonoBehaviour in the runtimePassiveBehaviors list
            var behaviorToRemove = character.runtimePassiveBehaviors
                .FirstOrDefault(b => b != null && b.name.Contains(prefabName));

            if (behaviorToRemove != null)
            {
                character.runtimePassiveBehaviors.Remove(behaviorToRemove);
                GameObject.Destroy(behaviorToRemove.gameObject);
            }
        }

        Debug.Log($"❎ Unequipped armor from {type}: {equippedArmor.armorName}");

        equippedArmor = null;
    }


}
