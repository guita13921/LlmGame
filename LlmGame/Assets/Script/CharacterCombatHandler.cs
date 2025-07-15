using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CharacterCombatHandler : MonoBehaviour
{
    private BattleManager battleManager;

    private void Awake()
    {
        battleManager = GetComponent<BattleManager>();
        if (battleManager == null)
        {
            Debug.LogError("CharacterCombatHandler requires BattleManager on the same GameObject.");
        }
    }

    #region Player

    public IEnumerator PlayerAttack(CharacterActionData chosenAction, float feasibility, float potential, string effectValue, string effectDesc, Character target)
    {
        var player = battleManager.currentActingCharacter as Player;
        if (player == null || target == null || !target.IsAlive())
        {
            Debug.LogWarning("Invalid player or target.");
            yield break;
        }

        float baseDamage = player.attack;
        float calculatedDamage = battleManager.damageCalculator.CalculateDamage(feasibility, potential, baseDamage, battleManager.lastUserMessage, player, target);
        int finalDamage = Mathf.RoundToInt(calculatedDamage);

        // ✅ Prepare damage
        player.pendingDamage = finalDamage;
        player.damageTarget = target;
        player.damagePortions = new List<float>(chosenAction.damagePortions);
        player.currentHitIndex = 0;

        // ✅ Play Animation
        yield return battleManager.WaitForAnimation(player, chosenAction.animationTrigger);

        // ✅ Log (เมื่อ animation จบ)
        string log = $"Turn {battleManager.turnCount}: {player.characterName} used {chosenAction.actionName} for total {finalDamage} damage → Target: {target.characterName}";
        battleManager.battleLog.Add(log);
        Debug.Log(log);

        yield return battleManager.StartCoroutine(battleManager.combatHandler.EndPlayerTurn());
    }



    public void UseItem(List<Item> item, string outcomeType)
    {
        Debug.Log($"Use Item :{item} -> {outcomeType}");
    }

    public IEnumerator EndPlayerTurn()
    {
        yield return new WaitForSeconds(2.0f);

        if (battleManager.CheckBattleEnd())
        {
            battleManager.battleActive = false;
            Debug.Log("Battle Finished!");
        }

        battleManager.turnCount++;
        battleManager.isActionPhase = false;
        battleManager.currentActingCharacter = null;
        battleManager.chatAI.HideInputUI();
    }

    #endregion

    #region Enemy

    // เริ่มต้นการโจมตีของศัตรู
    public void EnemyAttack(Character enemy, Character target, CharacterActionData chosenAction)
    {
        battleManager.StartCoroutine(battleManager.chatAI.SendEnemyMessage(enemy, target, chosenAction.actionName));
    }

    // จัดการผลของการโจมตี
    public IEnumerator ResolveEnemyAttack(Character enemy, Character target, CharacterActionData chosenAction, float feasibility, float potential, string effectValue, string effectDesc)
    {
        if (enemy == null || target == null || !target.IsAlive())
        {
            Debug.LogWarning("Invalid enemy or target.");
            yield break;
        }

        float baseDamage = enemy.attack;
        float calculatedDamage = battleManager.damageCalculator.CalculateDamageNoCreativity(feasibility, potential, baseDamage, enemy, target);
        int finalDamage = Mathf.RoundToInt(calculatedDamage);

        enemy.pendingDamage = finalDamage;
        enemy.damageTarget = target;
        enemy.damagePortions = new List<float>(chosenAction.damagePortions);
        enemy.currentHitIndex = 0;

        // ✅ Wait for animation to finish
        yield return battleManager.WaitForAnimation(enemy, chosenAction.animationTrigger);

        string log = $"Turn {battleManager.turnCount}: {enemy.characterName} used {chosenAction.actionName} for total {finalDamage} damage → Target: {target.characterName}";
        battleManager.battleLog.Add(log);
        Debug.Log(log);

        yield return battleManager.StartCoroutine(EndEnemyTurn());
    }



    private IEnumerator EndEnemyTurn()
    {
        yield return new WaitForSeconds(2.0f);

        if (battleManager.CheckBattleEnd())
        {
            battleManager.battleActive = false;
            Debug.Log("Battle Finished!");
        }

        battleManager.turnCount++;
        battleManager.isActionPhase = false;
        battleManager.currentActingCharacter = null;
    }

    #endregion
}
