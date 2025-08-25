using TMPro;
using UnityEngine;

public class LevelRewardsUI : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text moneyText;
    public TMP_Text itemsText;

    public void Show(int money, int items)
    {
        if (panel != null)
            panel.SetActive(true);
        if (moneyText != null)
            moneyText.text = $"Money Earned: {money}";
        if (itemsText != null)
            itemsText.text = $"Items Found: {items}";
    }
}
