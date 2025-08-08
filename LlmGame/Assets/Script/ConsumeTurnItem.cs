using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumeTurnItem : Item
{
    [Header("Animation Trigger")]
    public string animationTrigger;

    public virtual IEnumerator UseOnTarget(Character user, Character target, BattleManager battleManager)
    {
        if (user == null || target == null || battleManager == null)
        {
            Debug.LogError("[ConsumeTurnItem] Null reference: user, target, or battleManager is null.");
            yield break;
        }

        Debug.Log($"{user.characterName} uses {itemName} on {target.characterName}");

        if (!string.IsNullOrEmpty(animationTrigger))
        {
            yield return battleManager.StartCoroutine(battleManager.WaitForAnimation(user, animationTrigger));
        }
        else
        {
            yield return new WaitForSeconds(0.25f);
        }

        switch (itemName)
        {
            case "Reinforced Plating":
                target.defense += 10;
                battleManager.battleLog.Add($"{user.characterName} used {itemName} on {target.characterName}, increasing defense by 10.");
                break;

            case "HP Booster":
                target.maxHP = Mathf.RoundToInt(target.maxHP * 1.2f);
                target.currentHP = Mathf.RoundToInt(target.currentHP * 1.2f);
                if (target.currentHP > target.maxHP) target.currentHP = target.maxHP;
                battleManager.battleLog.Add($"{user.characterName} boosted {target.characterName}'s HP by 20%.");
                break;

            case "MP Cell":
                target.maxMP = Mathf.RoundToInt(target.maxMP * 1.2f);
                target.currentMP = Mathf.RoundToInt(target.currentMP * 1.2f);
                if (target.currentMP > target.maxMP) target.currentMP = target.maxMP;
                battleManager.battleLog.Add($"{user.characterName} boosted {target.characterName}'s MP by 20%.");
                break;

            case "Bandage":
                target.activeStatusEffects.RemoveAll(e => e.effectType == StatusEffectType.Bleed);
                battleManager.battleLog.Add($"{user.characterName} bandaged {target.characterName}, removing bleeding.");
                break;

            case "Antidote":
                target.activeStatusEffects.RemoveAll(e => e.effectType == StatusEffectType.Poison);
                battleManager.battleLog.Add($"{user.characterName} cured {target.characterName}'s poison.");
                break;

            case "Shield Battery":
                target.currentshield = Mathf.Min(target.maxShield, target.currentshield + 20);
                battleManager.battleLog.Add($"{user.characterName} restored 20 shield to {target.characterName}.");
                break;

            case "Focus Tonic":
                target.ApplyStatusEffect(new TurnStatusEffect(StatusEffectType.CritChanceUp, 2, 25));
                battleManager.battleLog.Add($"{user.characterName} increased {target.characterName}'s critical chance.");
                break;

            case "Adrenaline Shot":
                int heal25 = Mathf.RoundToInt(target.maxHP * 0.25f);
                target.currentHP = Mathf.Min(target.maxHP, target.currentHP + heal25);
                battleManager.battleLog.Add($"{user.characterName} healed {target.characterName} for {heal25} HP.");
                break;

            case "Soul Patch":
                int heal50 = Mathf.RoundToInt(target.maxHP * 0.5f);
                target.currentHP = Mathf.Min(target.maxHP, target.currentHP + heal50);
                battleManager.battleLog.Add($"{user.characterName} healed {target.characterName} for {heal50} HP.");
                break;

            case "Overdrive Chip":
                target.mpRegenPerTurn += 10;
                battleManager.battleLog.Add($"{user.characterName} enhanced {target.characterName}'s MP regeneration by 10 per turn.");
                break;

            default:
                Debug.LogWarning($"[ConsumeTurnItem] No effect implemented for {itemName}.");
                break;
        }

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