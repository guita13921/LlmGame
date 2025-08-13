using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Passive Item")]
public class PassiveItemData : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public Sprite icon; // <- Sprite for UI Image
    public ItemRarity rarity;
    public int value;
    public GameObject itemPrefab;

    /// <summary>
    /// Instantiates the passive item prefab on the character and applies its effect.
    /// </summary>
    public void EquipTo(Character character)
    {
        if (itemPrefab == null)
        {
            Debug.LogWarning($"{itemName} has no prefab assigned.");
            return;
        }

        // ✅ Instantiate the prefab
        GameObject instance = Instantiate(itemPrefab, character.transform);

        // ✅ Apply effect
        var passiveComponent = instance.GetComponent<IPassiveItem>();
        if (passiveComponent != null)
        {
            passiveComponent.ApplyEffect(character);
        }

        Debug.Log($"✅ Equipped passive item: {itemName}, tracked {instance.GetComponents<MonoBehaviour>().Length} behaviors.");
    }


}
