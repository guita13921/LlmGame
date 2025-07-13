using UnityEngine;
using UnityEngine.UI;

public class ItemButtonUI : MonoBehaviour
{
    public Item item;             // Reference to this item
    public BattleManager battleManager; // Assign via inspector or find automatically
    public Button button;

    private void Start()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnItemClick);
    }

    private void OnItemClick()
    {
        if (battleManager.player == null)
        {
            Debug.LogWarning("No player assigned in BattleManager!");
            return;
        }

        ActiveConsumeTurnItem(item);
    }

    public void ActiveConsumeTurnItem(Item item)
    {
        if (item == null) return;

        // Deactivate all other items
        foreach (var invItem in battleManager.player.activeItem)
        {
            invItem.isActive = false;
        }

        if (!battleManager.player.activeItem.Contains(item))
        {

            battleManager.player.activeItem.Add(item);
            battleManager.player.isUsingConsumeTurnItem = true;

            Debug.Log($"Item '{item.itemName}' activated by player!");

            // ✅ Fill input field with keywords
            if (battleManager.playerInputField != null)
            {
                battleManager.playerInputField.text = string.Empty;
                string keywordText = string.Join(", ", item.keyWords);
                battleManager.playerInputField.text = keywordText;
            }
        }
        else
        {
            battleManager.player.activeItem.Remove(item);
            battleManager.player.isUsingConsumeTurnItem = false;

            Debug.Log($"Item '{item.itemName}' deactivated by player!");

            // ✅ Clear input field
            if (battleManager.playerInputField != null)
            {
                battleManager.playerInputField.text = string.Empty;
            }
        }
    }
}
