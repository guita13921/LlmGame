using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Healing Item", menuName = "Inventory/HealingItem")]
public class HealingItem : ConsumeTurnItem
{
    [Header("Stat")]
    public int healingAmount;
    public override IEnumerator UseOnTarget(Character user, Character target, BattleManager battleManager)
    {
        if (user == null || target == null || battleManager == null)
        {
            Debug.LogError("[HealingItem] Null reference: user, target, or battleManager is null.");
            yield break;
        }

        Debug.Log($"{user.characterName} uses {itemName} on {target.characterName}");

        if (!string.IsNullOrEmpty(animationTrigger))
        {
            yield return battleManager.StartCoroutine(battleManager.WaitForAnimation(user, animationTrigger));
        }
        else
        {
            Debug.LogWarning("[HealingItem] animationTrigger is null or empty.");
            yield return new WaitForSeconds(0.25f); // fallback wait
        }

        target.currentHP = Mathf.Min(target.maxHP, target.currentHP + healingAmount);
        Debug.Log($"{target.characterName} healed for {healingAmount} HP");

        battleManager.battleLog.Add($"{user.characterName} used {itemName} on {target.characterName} and healed {healingAmount} HP");

        remain = Mathf.Max(0, remain - 1);

        user.activeItem.Clear();
        user.isUsingConsumeTurnItem = false;
        battleManager.isUsingConsumableMode = false;

        if (battleManager.playerInputField != null)
        {
            battleManager.playerInputField.text = "";
            battleManager.playerInputField.interactable = true;
        }

        battleManager.chatAI.ShowInputUI();
        battleManager.EndPlayerTurn();
    }


}
