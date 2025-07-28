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
        int finalDamage;

        if (battleManager.player.isUsingUltimateSkill == false)
        {
            float calculatedDamage = battleManager.damageCalculator.CalculateDamage(feasibility, potential, baseDamage, battleManager.lastUserMessage, player, target);
            Debug.Log("calculatedDamage : " + calculatedDamage);
            finalDamage = Mathf.RoundToInt(calculatedDamage);
        }
        else
        {
            float calculatedDamage = battleManager.damageCalculator.CalculateDamageNoCreativity(feasibility, potential, baseDamage, player, target);
            Debug.Log("calculatedDamage : " + calculatedDamage);
            finalDamage = Mathf.RoundToInt(calculatedDamage);
        }

        // ✅ Prepare damage
        player.pendingDamage = finalDamage;
        player.damageTarget = target;
        player.currentHitIndex = 0;


        // ✅ Assign selected action so hits can use hitEffects
        player.selectedAction = chosenAction;

        // ✅ Play Animation
        if (battleManager.player.isUsingUltimateSkill == true)
        {
            yield return battleManager.WaitForAnimation(player, battleManager.player.currentSkill.aniamtionTrigger);
        }
        else
        {
            yield return battleManager.WaitForAnimation(player, chosenAction.animationTrigger);
        }

        // ✅ Log
        string log = $"Turn {battleManager.turnCount}: {player.characterName} {battleManager.playerInputField.text}  → Target: {target.characterName}";
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
        enemy.currentHitIndex = 0;

        // ✅ Assign selected action
        enemy.selectedAction = chosenAction;

        // ✅ Wait for animation to finish
        yield return battleManager.WaitForAnimation(enemy, chosenAction.animationTrigger);

        string log = $"Turn {battleManager.turnCount}: {enemy.characterName} used {chosenAction.actionName}  → Target: {target.characterName}";
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
