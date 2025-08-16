using UnityEngine;
using TMPro;

public class PassiveItemTooltip : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI rarityText;

    private RectTransform rectTransform;
    private Canvas rootCanvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();

        HideTooltip();
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;

        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            Input.mousePosition,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out anchoredPos
        );

        // Optional: offset for visibility
        anchoredPos += new Vector2(10f, -10f);

        rectTransform.anchoredPosition = anchoredPos;
    }

    public void ShowTooltip(string name, string description, string rarity)
    {
        nameText.text = name;
        descriptionText.text = description;
        //rarityText.text = rarity;

        gameObject.SetActive(true);
    }

    public void HideTooltip()
    {
        gameObject.SetActive(false);
    }
}
