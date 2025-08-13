using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TreasureUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TreasureSystem treasureSystem;
    [SerializeField] private Player player;
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject panel;

    [Header("Buttons")]
    [SerializeField] private Button grantAndShowButton;
    [SerializeField] private Button nextSceneButton;

    private void Awake()
    {
        if (grantAndShowButton != null)
            grantAndShowButton.onClick.AddListener(OnClick_GrantTreasureAndShow);

        if (nextSceneButton != null)
        {
            nextSceneButton.gameObject.SetActive(false); // start hidden
            nextSceneButton.onClick.AddListener(OnClick_LoadMapGenerate);
        }
    }

    private void OnEnable()
    {
        // Optional: start hidden until something is granted
        if (panel != null) panel.SetActive(true);
    }

    /// <summary>
    /// Grants treasure and updates the UI with icon + description.
    /// </summary>
    public void OnClick_GrantTreasureAndShow()
    {
        Debug.Log($"[UI] Grant&Show from controller {GetInstanceID()} player={player?.name}");
        if (treasureSystem == null || player == null)
        {
            Debug.LogWarning("TreasureUIController: Missing TreasureSystem or Player reference.");
            ShowEmpty("Missing refs", "Treasure system or player not set.");
            return;
        }

        var itemSo = treasureSystem.GrantTreasure(player);
        if (itemSo == null)
        {
            ShowEmpty("Nothing found", "No more items available.");
            return;
        }

        if (itemSo is PassiveItemData passiveItem)
        {
            ApplyUI(passiveItem.icon, passiveItem.itemName, passiveItem.description);
        }
        else if (itemSo is ArmorData armorItem)
        {
            ApplyUI(armorItem.icon, armorItem.armorName, armorItem.description);
        }
        else
        {
            ApplyUI(null, itemSo.name, "No description available.");
        }

        // Hide the grant button
        if (grantAndShowButton != null)
            grantAndShowButton.gameObject.SetActive(false);

        // Show the next button
        if (nextSceneButton != null)
            nextSceneButton.gameObject.SetActive(true);
    }

    private void ApplyUI(Sprite icon, string title, string desc)
    {
        if (panel != null) panel.SetActive(true);

        if (iconImage != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = icon != null;
        }

        if (nameText != null) nameText.text = title ?? string.Empty;
        if (descriptionText != null) descriptionText.text = desc ?? string.Empty;
    }

    private void ShowEmpty(string title, string desc)
    {
        ApplyUI(null, title, desc);
    }

    private void OnClick_LoadMapGenerate()
    {
        SceneManager.LoadScene("MapGenerate");
    }
}
