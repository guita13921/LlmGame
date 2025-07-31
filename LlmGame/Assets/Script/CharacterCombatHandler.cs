using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class CharacterCombatHandler : MonoBehaviour
{
    private BattleManager battleManager;
    public TMP_Text responseText;

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
            DamageResult result = battleManager.damageCalculator.CalculateDamageNoCreativity(feasibility, potential, baseDamage, player, target);
            Debug.Log("calculatedDamage : " + result.damage);
            finalDamage = Mathf.RoundToInt(result.damage);

        }
        else
        {
            DamageResult result = battleManager.damageCalculator.CalculateDamageNoCreativity(feasibility, potential, baseDamage, player, target);
            Debug.Log("calculatedDamage : " + result.damage);
            finalDamage = Mathf.RoundToInt(result.damage);
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
        string log = $"Turn {battleManager.turnCount}: {player.characterName} {battleManager.playerInputField.text}  → Target: {target.characterName} Result: {target.currentHP} / {target.maxHP} ({battleManager.chatAI.baseEffect})";
        battleManager.battleLog.Add(log);
        Debug.Log(log);

        Debug.Log(target.GetBodyPartStatus());

        yield return battleManager.StartCoroutine(battleManager.combatHandler.EndPlayerTurn());
    }


    public void UseItem(List<Item> item, string outcomeType)
    {
        Debug.Log($"Use Item :{item} -> {outcomeType}");
    }

    public IEnumerator EndPlayerTurn()
    {
        yield return new WaitForSeconds(2.0f);

        // ✅ Trigger turn-end logic for player
        if (battleManager.currentActingCharacter is Player player)
        {
            // 🔁 Runtime-attached MonoBehaviours (instantiated)
            foreach (var listener in player.GetComponents<ITurnListener>())
            {
                listener.OnTurnEnd(player);
            }

            // 🔁 PassiveItemData prefabs (optional if stateless)
            foreach (var itemData in player.equippedPassiveItems)
            {
                if (itemData.itemPrefab == null) continue;

                ITurnListener prefabListener = itemData.itemPrefab.GetComponent<ITurnListener>();
                if (prefabListener != null)
                {
                    prefabListener.OnTurnEnd(player);
                }
            }
        }

        // ✅ End battle check
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
        DamageResult result = battleManager.damageCalculator.CalculateDamageNoCreativity(feasibility, potential, baseDamage, enemy, target);
        int finalDamage = Mathf.RoundToInt(result.damage);

        enemy.pendingDamage = finalDamage;
        enemy.damageTarget = target;
        enemy.currentHitIndex = 0;

        // ✅ Assign selected action
        enemy.selectedAction = chosenAction;

        // ✅ Wait for animation to finish
        yield return battleManager.WaitForAnimation(enemy, chosenAction.animationTrigger);

        string log = $"Turn {battleManager.turnCount}: {enemy.characterName} used {chosenAction.actionName}  → Target: {target.characterName} Result: {target.currentHP} / {target.maxHP} ({battleManager.chatAI.baseEffect})";
        battleManager.battleLog.Add(log);
        Debug.Log(log);

        yield return battleManager.StartCoroutine(EndEnemyTurn());
    }


    private IEnumerator EndEnemyTurn()
    {
        yield return new WaitForSeconds(2.0f);

        // ✅ End battle check
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

    public string TryApplyStatusEffects(Character attacker, Character target)
    {
        List<string> appliedEffects = new();

        // 🩸 Bleed chance roll
        if (attacker.possibilityPool.Roll(StatusChanceType.Bleed))
        {
            int duration = target.characterType == CharacterType.Human ? 2 : 1;

            TurnStatusEffect bleed = new TurnStatusEffect(
                StatusEffectType.Bleed,
                duration,
                1,
                attacker
            );

            target.ApplyStatusEffect(bleed);
            appliedEffects.Add($"inflicted Bleed on {target.characterName} for {duration} turn(s)");
        }

        // ☠️ Poison chance roll
        if (attacker.possibilityPool.Roll(StatusChanceType.Poison))
        {
            TurnStatusEffect poison = new TurnStatusEffect(StatusEffectType.Poison, 3, 1);
            target.ApplyStatusEffect(poison);

            appliedEffects.Add($"inflicted Poison on {target.characterName}");
        }

        // 💬 Return effects in narration-style format
        if (appliedEffects.Count > 0)
        {
            return " Effects: " + string.Join(". ", appliedEffects) + ".";
        }
        else
        {
            return " No special effects.";
        }
    }

}
