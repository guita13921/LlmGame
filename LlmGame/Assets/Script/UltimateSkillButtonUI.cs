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

    private void Start()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnUltimateSkillClick);

        if (tooltip == null)
            tooltip = FindObjectOfType<UltimateSkillTooltip>();
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
    }

    private void ToggleUltimateSkill(DamageModifierSkill skill)
    {
        if (skill == null)
        {
            Debug.LogWarning("Ultimate skill not assigned!");
            return;
        }

        if (!battleManager.isActionPhase || battleManager.currentActingCharacter != battleManager.player)
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
}
