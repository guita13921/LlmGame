using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("References")]
    public Player player; // Drag your Player object here in the inspector
    [Header("HP UI")]
    public Image hpFill; // Image with Fill Method set to Horizontal
    public TMP_Text hpCountText;

    [Header("MP UI")]
    public Image mpFill; // Image with Fill Method set to Horizontal
    public TMP_Text mpCountText;

    private void Start()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }

        UpdateUI();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (player == null) return;

        // === HP ===
        float hpPercent = (player.currentHP <= 0 || player.maxHP <= 0) ? 0f : (float)player.currentHP / player.maxHP;
        if (hpFill != null)
            hpFill.fillAmount = hpPercent;

        if (hpCountText != null)
            hpCountText.text = $"{player.currentHP} / {player.maxHP} ({Mathf.RoundToInt(hpPercent * 100f)}%)";

        // === MP ===
        float mpPercent = (player.currentMP <= 0 || player.maxMP <= 0) ? 0f : (float)player.currentMP / player.maxMP;
        if (mpFill != null)
            mpFill.fillAmount = mpPercent;

        if (mpCountText != null)
            mpCountText.text = $"{player.currentMP} / {player.maxMP} ({Mathf.RoundToInt(mpPercent * 100f)}%)";
    }
}
