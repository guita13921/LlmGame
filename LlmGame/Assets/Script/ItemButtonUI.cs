using UnityEngine;
using UnityEngine.UI;

public class ItemButtonUI : MonoBehaviour
{
    [Header("References")]
    public Item item;                     // Reference to this item (set in inspector or dynamically)
    public BattleManager battleManager;   // Assign via inspector or find automatically
    public Button button;
    public Image iconImage;               // UI Image to display the item icon

    private void Start()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnItemClick);

        // Show the icon if available
        if (iconImage != null && item != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false; // Hide if no icon
        }
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
        // ✅ Only allow activation during player's turn
        if (!battleManager.isActionPhase || battleManager.currentActingCharacter != battleManager.player)
        {
            Debug.LogWarning("Cannot use items outside your turn!");
            return;
        }

        if (item == null) return;

        battleManager.selectedTarget = null;

        // Deactivate all other items
        foreach (var invItem in battleManager.player.activeItem)
        {
            invItem.isActive = false;
        }

        if (!battleManager.player.activeItem.Contains(item))
        {
            battleManager.player.activeItem.Add(item);
            battleManager.player.isUsingConsumeTurnItem = true;
            battleManager.isUsingConsumableMode = true;

            if (battleManager.playerInputField != null)
            {
                battleManager.playerInputField.text = string.Join(", ", item.keyWords);
                battleManager.playerInputField.interactable = false;
            }

            battleManager.chatAI.HideInputUI(); // Hide AI input panel
        }
        else
        {
            battleManager.player.activeItem.Remove(item);
            battleManager.player.isUsingConsumeTurnItem = false;
            battleManager.isUsingConsumableMode = false;

            Debug.Log($"Item '{item.itemName}' deactivated by player!");

            if (battleManager.playerInputField != null)
            {
                battleManager.playerInputField.text = "";
                battleManager.playerInputField.interactable = true;
            }

            battleManager.chatAI.ShowInputUI();
        }
    }
}
