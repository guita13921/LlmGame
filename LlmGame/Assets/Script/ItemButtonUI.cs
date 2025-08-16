using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ItemButtonUI : MonoBehaviour
{
    [Header("References")]
    public Item item;
    public BattleManager battleManager;
    public Button button;
    public Image iconImage;

    private void Start()
    {

        // Assign button
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnItemClick);

        // Setup icon
        if (iconImage != null && item != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
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
        if (!battleManager.isActionPhase || battleManager.currentActingCharacter != battleManager.player)
        {
            Debug.LogWarning("Cannot use items outside your turn!");
            return;
        }

        if (item == null) return;

        battleManager.selectedTarget = null;

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
                battleManager.playerInputField.text = string.Join(", ", item.name, " ", item.itemDescription);
                battleManager.playerInputField.interactable = false;
            }

            battleManager.chatAI.HideInputUI();
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
