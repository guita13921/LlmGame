using UnityEngine;
using UnityEngine.UI;

public class UltimateSkillButtonUI : MonoBehaviour
{
    public DamageModifierSkill ultimateSkill;         // Assign via inspector
    public BattleManager battleManager;               // Reference to the BattleManager
    public Button button;                             // UI Button

    private void Start()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(OnUltimateSkillClick);
    }

    private void OnUltimateSkillClick()
    {
        if (battleManager.player == null)
        {
            Debug.LogWarning("No player assigned in BattleManager!");
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

        // ✅ Check action phase and turn
        if (!battleManager.isActionPhase || battleManager.currentActingCharacter != battleManager.player)
        {
            Debug.LogWarning("Cannot use skills outside your turn!");
            return;
        }

        // ✅ Check if player has the skill
        if (!battleManager.player.damageModifierSkills.Contains(skill))
        {
            Debug.LogWarning("This skill is not available to the player!");
            return;
        }

        battleManager.selectedTarget = null;

        // Toggle off if already selected
        if (battleManager.player.isUsingUltimateSkill && battleManager.player.currentSkill == skill)
        {
            battleManager.player.currentSkill = null;
            battleManager.player.isUsingUltimateSkill = false;

            // Restore input field
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
            // Activate the skill
            battleManager.player.currentSkill = skill;
            battleManager.player.isUsingUltimateSkill = true;

            // Update input UI
            if (battleManager.playerInputField != null)
            {
                battleManager.playerInputField.text = $"ULTIMATE: {skill.skillName}";
                battleManager.playerInputField.interactable = false;
            }

            battleManager.chatAI.HideInputUI();
            Debug.Log($"Ultimate skill '{skill.skillName}' activated.");
        }
    }
}
