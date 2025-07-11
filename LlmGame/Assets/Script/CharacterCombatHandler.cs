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

    public void PlayerAttack(float feasibility, float potential, string effectValue, string effectDesc)
    {
        var player = battleManager.currentActingCharacter as Player;
        if (player == null)
        {
            Debug.LogWarning("Not player's turn!");
            return;
        }

        var target = battleManager.GetRandomOpponent(player);
        if (target != null)
        {
            float baseDamage = player.attack;
            float calculatedDamage = battleManager.damageCalculator.CalculateDamage(feasibility, potential, baseDamage, battleManager.lastUserMessage, player, target);

            int finalDamage = Mathf.RoundToInt(calculatedDamage);
            target.TakeDamage(finalDamage);

            // ✅ Updated: use the new breakdown
            var weaponDamageBreakdown = battleManager.damageCalculator.GetActiveWeaponDamageBreakdown(player);
            float activeWeaponDamage = weaponDamageBreakdown.Values.Sum();
            string weaponInfo = activeWeaponDamage > 0 ? $" (Base: {baseDamage}, Weapon: +{activeWeaponDamage})" : "";

            string log = $"Turn {battleManager.turnCount}: {player.characterName} (HP: {player.currentHP}) " +
                         $"used \"{battleManager.lastUserMessage}\" [EffectValue: {effectValue}, EffectDesc: {effectDesc}] " +
                         $"for {finalDamage} damage{weaponInfo} → Target: {target.characterName} (HP: {target.currentHP} / {target.maxHP})";

            battleManager.battleLog.Add(log);
            Debug.Log(log);
        }

        battleManager.StartCoroutine(EndPlayerTurn());
    }


    private IEnumerator EndPlayerTurn()
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

    public void EnemyAttack(Character enemy, Character target, string proposedAction)
    {
        battleManager.StartCoroutine(battleManager.chatAI.SendEnemyMessage(enemy, target, proposedAction));
    }

    public void ResolveEnemyAttack(Character enemy, Character target, float feasibility, float potential, string effectValue, string effectDesc)
    {
        float baseDamage = enemy.attack;
        float calculatedDamage = battleManager.damageCalculator.CalculateDamageNoCreativity(feasibility, potential, baseDamage, enemy, target);

        int finalDamage = Mathf.RoundToInt(calculatedDamage);
        target.TakeDamage(finalDamage);

        // ✅ Updated: use new breakdown
        var weaponDamageBreakdown = battleManager.damageCalculator.GetActiveWeaponDamageBreakdown(enemy);
        float activeWeaponDamage = weaponDamageBreakdown.Values.Sum();
        string weaponInfo = activeWeaponDamage > 0 ? $" (Base: {baseDamage}, Weapon: +{activeWeaponDamage})" : "";

        string log = $"Turn {battleManager.turnCount}: {enemy.characterName} (HP: {enemy.currentHP}) " +
                     $"used [EffectValue: {effectValue}, EffectDesc: {effectDesc}] " +
                     $"for {finalDamage} damage{weaponInfo} → Target: {target.characterName} (HP: {target.currentHP})";

        battleManager.battleLog.Add(log);
        Debug.Log(log);

        battleManager.StartCoroutine(EndEnemyTurn());
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
