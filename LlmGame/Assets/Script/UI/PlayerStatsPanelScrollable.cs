using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class PlayerStatsPanelScrollable : MonoBehaviour
{
    [Header("References")]
    public Player player;                  // Auto-found if not set
    public GameObject panelRoot;           // Panel GameObject to show/hide; defaults to this GO
    public Button exitButton;              // Button to close panel
    public ScrollRect scrollRect;          // ScrollRect (from a UI -> Scroll View)
    public Scrollbar verticalScrollbar;    // Optional: assign your scrollbar
    public TMP_Text contentText;           // TextMeshPro inside Content

    [Header("Behavior")]
    public bool startHidden = true;
    public bool resetScrollOnOpen = true;          // if true, go to top when opening
    public bool keepScrollPositionOnRefresh = true; // keep user scroll when content refreshes

    private void Awake()
    {
        // References
        if (player == null)
            player = FindObjectOfType<Player>();

        if (panelRoot == null)
            panelRoot = gameObject;

        // Exit button
        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
            exitButton.onClick.AddListener(Close);
        }

        // Hook scrollbar to ScrollRect (optional)
        if (scrollRect != null && verticalScrollbar != null)
        {
            scrollRect.verticalScrollbar = verticalScrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
        }

        if (startHidden && panelRoot != null)
            panelRoot.SetActive(false);
    }

    private void OnEnable()
    {
        // Only refresh; do NOT reset scroll here (prevents snapping back)
        Refresh();
    }

    public void Open()
    {
        if (panelRoot == null) return;

        panelRoot.SetActive(true);
        Refresh();

        if (resetScrollOnOpen)
            ResetScrollToTop();
    }

    public void Close()
    {
        if (panelRoot == null) return;
        panelRoot.SetActive(false);
    }

    public void Toggle()
    {
        if (panelRoot == null) return;
        if (panelRoot.activeSelf) Close(); else Open();
    }

    private void ResetScrollToTop()
    {
        if (scrollRect != null)
            scrollRect.verticalNormalizedPosition = 1f;
        if (verticalScrollbar != null)
            verticalScrollbar.value = 1f;
    }

    public void Refresh()
    {
        if (player == null || contentText == null) return;

        // Preserve current scroll pos if requested
        float prevPos = 1f;
        if (keepScrollPositionOnRefresh && scrollRect != null)
            prevPos = scrollRect.verticalNormalizedPosition;

        // ==== Build text ====
        int effAtk = player.attack + player.bonusAttack;
        int effDef = player.defense + player.bonusDefense;
        int effFoc = player.focus + player.bonusFocus;
        int effHPMax = player.maxHP + player.bonusMaxHP;
        int effMPMax = player.maxMP + player.bonusMaxMP;
        float effSpd = player.speed + player.bonusSpeed;
        int effShMax = player.maxShield + player.bonusMaxShield;

        float hpPct = effHPMax > 0 ? (player.currentHP / (float)effHPMax) * 100f : 0f;
        float mpPct = effMPMax > 0 ? (player.currentMP / (float)effMPMax) * 100f : 0f;

        var sb = new StringBuilder();

        sb.AppendLine($"<size=130%><b>{player.characterName}</b></size>");
        sb.AppendLine($"Type: {player.characterType}");
        sb.AppendLine();

        sb.AppendLine("<b>Resources</b>");
        sb.AppendLine($"Money: {player.money}");
        sb.AppendLine($"HP: {player.currentHP} / {effHPMax} ({Mathf.RoundToInt(hpPct)}%)");
        sb.AppendLine($"MP: {player.currentMP} / {effMPMax} ({Mathf.RoundToInt(mpPct)}%)");
        sb.AppendLine($"Shield: {player.currentshield} / {effShMax}");
        sb.AppendLine();

        sb.AppendLine("<b>Stats</b>");
        sb.AppendLine($"Attack:  {effAtk}  (base {player.attack}  +{player.bonusAttack})");
        sb.AppendLine($"Defense: {effDef}  (base {player.defense} +{player.bonusDefense})");
        sb.AppendLine($"Focus:   {effFoc}  (base {player.focus}   +{player.bonusFocus})");
        sb.AppendLine($"Speed:   {effSpd}  (base {player.speed}   +{player.bonusSpeed})");
        sb.AppendLine($"Max HP:  {effHPMax} (base {player.maxHP}  +{player.bonusMaxHP})");
        sb.AppendLine($"Max MP:  {effMPMax} (base {player.maxMP}  +{player.bonusMaxMP})");
        sb.AppendLine($"Max Shield: {effShMax} (base {player.maxShield} +{player.bonusMaxShield})");
        sb.AppendLine();

        sb.AppendLine("<b>Status Effects</b>");
        if (player.activeStatusEffects == null || player.activeStatusEffects.Count == 0)
        {
            sb.AppendLine("None");
        }
        else
        {
            foreach (var e in player.activeStatusEffects)
            {
                string turns = e.isPermanent ? "∞" : (e.remainingTurns + "t");
                sb.AppendLine($"- {e.effectType} ({turns})");
            }
        }

        sb.AppendLine();
        sb.AppendLine("<b>Body Parts</b>");
        sb.AppendLine(player.GetBodyPartStatus());

        contentText.text = sb.ToString();

        // Rebuild so ScrollRect updates content height
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentText.rectTransform);

        // Restore scroll pos if requested
        if (keepScrollPositionOnRefresh && scrollRect != null)
            scrollRect.verticalNormalizedPosition = prevPos;
    }

    private void Update()
    {
        // Optional: close with Escape
        if (panelRoot != null && panelRoot.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }
}
