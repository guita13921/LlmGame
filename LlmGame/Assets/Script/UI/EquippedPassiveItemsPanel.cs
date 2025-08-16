using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class EquippedPassiveItemsPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject passiveItemPrefab;
    [SerializeField] private Transform passiveItemContainer;
    [SerializeField] private PassiveItemTooltip tooltip; // Assign in Inspector


    [Header("Toggle Buttons")]
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (openButton != null)
            openButton.onClick.AddListener(OpenPanel);

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
    }

    private void Start()
    {
        // Initial state: panel closed
        gameObject.SetActive(false);
        if (openButton != null) openButton.gameObject.SetActive(true);
        if (closeButton != null) closeButton.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        RefreshPanel();
    }

    public void RefreshPanel()
    {
        foreach (Transform child in passiveItemContainer)
        {
            Destroy(child.gameObject);
        }

        List<PassiveItemData> passiveItems = PlayerData.Instance.equippedPassiveItems;

        foreach (PassiveItemData item in passiveItems)
        {
            GameObject entry = Instantiate(passiveItemPrefab, passiveItemContainer);
            PassiveItemUIEntry uiEntry = entry.GetComponent<PassiveItemUIEntry>();

            if (uiEntry != null)
            {
                uiEntry.Initialize(item, tooltip);
            }
            else
            {
                Debug.LogWarning("PassiveItemUIEntry component is missing on prefab.");
            }
        }
    }

    public void OpenPanel()
    {
        gameObject.SetActive(true);
        RefreshPanel();

        if (openButton != null) openButton.gameObject.SetActive(false);
        if (closeButton != null) closeButton.gameObject.SetActive(true);
    }

    public void ClosePanel()
    {
        gameObject.SetActive(false);

        if (openButton != null) openButton.gameObject.SetActive(true);
        if (closeButton != null) closeButton.gameObject.SetActive(false);
    }
}
