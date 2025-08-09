using System.Collections.Generic;
using UnityEngine;

public class TreasureSystem : MonoBehaviour
{
    public StoreSystem storeReference;

    [Header("Fallback (duplicates)")]
    [SerializeField] private bool allowDuplicatesWhenEmpty = true;
    [SerializeField] private List<PassiveItemData> duplicatePool; // assign all possible items here

    public ScriptableObject GrantTreasure(Player player)
    {
        if (storeReference == null)
        {
            Debug.LogWarning("TreasureSystem requires StoreSystem reference");
            return null;
        }

        List<ScriptableObject> stock = storeReference.GenerateStock(player, 1);
        if (stock != null && stock.Count > 0)
        {
            var item = stock[0];
            storeReference.BuyItem(player, item);
            return item;
        }

        // Fallback: allow duplicates from a known pool
        if (allowDuplicatesWhenEmpty && duplicatePool != null && duplicatePool.Count > 0)
        {
            int idx = Random.Range(0, duplicatePool.Count);
            var item = duplicatePool[idx];
            // Optional: if your store needs to process ownership/effects:
            storeReference.BuyItem(player, item);
            return item;
        }

        return null;
    }
}
