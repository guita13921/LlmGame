using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Items/Passive Item")]
public class PassiveItemData : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;

    public GameObject itemPrefab; // Prefab that contains a MonoBehaviour implementing IPassiveItem

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

        GameObject instance = Instantiate(itemPrefab, character.transform);
        var passiveComponent = instance.GetComponent<IPassiveItem>();

        if (passiveComponent != null)
        {
            passiveComponent.ApplyEffect(character);
        }
        else
        {
            Debug.LogWarning($"{itemName} prefab does not have an IPassiveItem component.");
        }
    }
}
