using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles treasure rewards, granting the player a random item they don't already own.
/// Uses the StoreSystem's pools for item selection.
/// </summary>
public class TreasureSystem : MonoBehaviour
{
    public StoreSystem storeReference;

    /// <summary>
    /// Grants a random item to the player.
    /// </summary>
    public ScriptableObject GrantTreasure(Player player)
    {
        if (storeReference == null)
        {
            Debug.LogWarning("TreasureSystem requires StoreSystem reference");
            return null;
        }

        List<ScriptableObject> stock = storeReference.GenerateStock(player, 1);
        if (stock.Count == 0) return null;

        ScriptableObject item = stock[0];
        storeReference.BuyItem(player, item);
        return item;
    }
}
