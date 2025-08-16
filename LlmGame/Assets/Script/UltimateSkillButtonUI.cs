using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UltimateSkillButtonUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public DamageModifierSkill ultimateSkill;
    public BattleManager battleManager;
    public Button button;

    [Header("Tooltip")]
    public UltimateSkillTooltip tooltip; // Assign in inspector

    [Header("Visuals")]
    [SerializeField] private Image iconImage;        // Optional: assign your button/icon image
    [SerializeField] private CanvasGroup canvasGroup; // Optional: smoother dimming if available
    [SerializeField] private float dimAlpha = 0.4f;  // Alpha when dimmed
    [SerializeField] private bool disableWhenInsufficientMP = true;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

    }
    private void Start()
    {
        button.onClick.AddListener(OnUltimateSkillClick);

        if (tooltip == null)
            tooltip = FindObjectOfType<UltimateSkillTooltip>();

        UpdateVisualState();
    }

    private void OnEnable()
    {
        UpdateVisualState();
    }

    private void Update()
    {
        // If MP or turn changes during gameplay, keep visuals in sync
        UpdateVisualState();
    }

    private void OnUltimateSkillClick()
    {
        if (battleManager.player == null)
        {
            Debug.Log("No player assigned in BattleManager!");
            return;
        }

        if (battleManager.player.currentMP < ultimateSkill.mpCost)
        {
            Debug.Log($"{battleManager.player.characterName} does not have enough MP to use {ultimateSkill.mpCost}!");
            return;
        }

        ToggleUltimateSkill(ultimateSkill);
        UpdateVisualState();
    }

    private void ToggleUltimateSkill(DamageModifierSkill skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("Ultimate skill not assigned!");
            return;
        }

        if (!IsPlayersTurn())
        {
            Debug.LogWarning("Cannot use skills outside your turn!");
            return;
        }

        if (!battleManager.player.damageModifierSkills.Contains(skill))
        {
            Debug.LogWarning("This skill is not available to the player!");
            return;
        }

        battleManager.selectedTarget = null;

        if (battleManager.player.isUsingUltimateSkill && battleManager.player.currentSkill == skill)
        {
            battleManager.player.currentSkill = null;
            battleManager.player.isUsingUltimateSkill = false;

            if (battleManager.playerInputField != null)
            {
                battleManager.playerInputField.text = "";
                battleManager.playerInputField.interactable = true;
            }

            battleManager.chatAI.ShowInputUI();
            Debug.Log($"Ultimate skill '{skill.skillName}' deactivated.");
        }
        else
        {
            battleManager.player.currentSkill = skill;
            battleManager.player.isUsingUltimateSkill = true;

            if (battleManager.playerInputField != null)
            {
                battleManager.playerInputField.text = $"ULTIMATE: {skill.skillName}";
                battleManager.playerInputField.interactable = false;
            }

            battleManager.chatAI.HideInputUI();
            Debug.Log($"Ultimate skill '{skill.skillName}' activated.");
        }
    }

    // === Tooltip Logic ===
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null && ultimateSkill != null)
        {
            tooltip.ShowTooltip(ultimateSkill, transform as RectTransform);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip?.HideTooltip();
    }

    // === Visual State ===
    private void UpdateVisualState()
    {
        if (battleManager == null || battleManager.player == null || ultimateSkill == null)
            return;

        bool playersTurn = IsPlayersTurn();
        bool hasMP = battleManager.player.currentMP >= ultimateSkill.mpCost;

        // Dim if it's the player's turn but they don't have enough MP.
        bool shouldDim = playersTurn && !hasMP;

        // Interactability (optional)
        if (disableWhenInsufficientMP)
        {
            button.interactable = playersTurn && hasMP;
        }
        else
        {
            button.interactable = playersTurn; // still let them click even without MP, if you prefer
        }

        // Apply dim
        if (canvasGroup != null)
        {
            canvasGroup.alpha = shouldDim ? dimAlpha : 1f;
        }
        else if (iconImage != null)
        {
            var c = iconImage.color;
            c.a = shouldDim ? dimAlpha : 1f;
            iconImage.color = c;
        }
    }

    private bool IsPlayersTurn()
    {
        return battleManager.isActionPhase && battleManager.currentActingCharacter == battleManager.player;
    }
}
