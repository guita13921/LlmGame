using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class CharacterCombatHandler : MonoBehaviour
{
    private BattleManager battleManager;
    private Dictionary<Character, Dictionary<DamageType, float>> lastDamageBreakdown = new();

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
        if (battleManager.showDebug) Debug.Log("PlayerAttack");

        var player = battleManager.currentActingCharacter as Player;
        if (player == null || target == null || !target.IsAlive())
        {
            Debug.LogWarning("Invalid player or target.");
            yield break;
        }

        float baseDamage = player.attack;
        int calculatedDamage;

        DamageResult result;
        if (battleManager.player.isUsingUltimateSkill)
        {
            result = battleManager.damageCalculator.CalculateDamageNoCreativity(feasibility, potential, baseDamage, player, target);
        }
        else
        {
            result = battleManager.damageCalculator.CalculateDamage(feasibility, potential, baseDamage, battleManager.lastUserMessage, player, target);
        }

        calculatedDamage = Mathf.RoundToInt(result.damage);
        int finalDamage = calculatedDamage;

        foreach (var behavior in player.runtimePassiveBehaviors)
        {
            if (behavior is IDamageReaction reaction)
                reaction.OnBeforeDamage(player, target, ref finalDamage);
        }

        if (target is Player targetPlayer)
        {
            foreach (var behavior in targetPlayer.runtimePassiveBehaviors)
            {
                if (behavior is IDamageReaction reaction)
                    reaction.OnBeforeDamage(player, targetPlayer, ref finalDamage);
            }

            foreach (var bodyPart in targetPlayer.bodyParts)
            {
                var armor = bodyPart.equippedArmor;
                if (armor == null || armor.itemBehaviorPrefab == null) continue;

                var reaction = armor.itemBehaviorPrefab.GetComponent<IDamageReaction>();
                if (reaction != null)
                {
                    reaction.OnBeforeDamage(player, targetPlayer, ref finalDamage);
                }
            }
        }

        player.pendingDamage = finalDamage;
        player.damageTarget = target;
        player.currentHitIndex = 0;
        player.selectedAction = chosenAction;

        if (player.isUsingUltimateSkill)
        {
            yield return battleManager.WaitForAnimation(player, battleManager.player.currentSkill.aniamtionTrigger);
        }
        else
        {
            yield return battleManager.WaitForAnimation(player, chosenAction.animationTrigger);
        }

        yield return new WaitForSeconds(0.75f); // 🔸 Delay after animation

        if (target is Player targetAfter)
        {
            foreach (var behavior in targetAfter.runtimePassiveBehaviors)
            {
                if (behavior is IDamageReaction reaction)
                    reaction.OnAfterDamage(player, targetAfter, finalDamage);
            }

            foreach (var bodyPart in targetAfter.bodyParts)
            {
                var armor = bodyPart.equippedArmor;
                if (armor == null || armor.itemBehaviorPrefab == null) continue;

                var reaction = armor.itemBehaviorPrefab.GetComponent<IDamageReaction>();
                if (reaction != null)
                {
                    reaction.OnAfterDamage(player, targetAfter, finalDamage);
                }
            }
        }

        foreach (var behavior in player.runtimePassiveBehaviors)
        {
            if (behavior is IDamageReaction reaction)
                reaction.OnAfterDamage(player, target, finalDamage);
        }

        yield return new WaitForSeconds(0.25f); // 🔸 Short pause before logging

        string log = $"Turn {battleManager.turnCount}: {player.characterName} {battleManager.playerInputField.text}  → Target: {target.characterName} Result: {target.currentHP} / {target.maxHP} ({battleManager.chatAI.baseEffect})";
        battleManager.battleLog.Add(log);

        yield return new WaitForSeconds(0.75f); // 🔸 Let player read log

        yield return battleManager.StartCoroutine(battleManager.combatHandler.EndPlayerTurn());
    }

    public IEnumerator EndPlayerTurn()
    {
        yield return new WaitForSeconds(0.5f); // 🔸 Small delay before cleanup

        if (battleManager.currentActingCharacter is Player player)
        {
            foreach (var listener in player.GetComponents<ITurnListener>())
                listener.OnTurnEnd(player);

            foreach (var listener in player.runtimePassiveBehaviors.OfType<ITurnListener>())
                listener.OnTurnEnd(player);
        }

        if (battleManager.CheckBattleEnd())
        {
            battleManager.battleActive = false;
            Debug.Log("Battle Finished!");
        }

        Debug.Log("EndPlayerTurn");
        battleManager.currentActingCharacter?.ClearEndTurnEffects();
        battleManager.turnCount++;
        battleManager.isActionPhase = false;
        battleManager.currentActingCharacter = null;
        battleManager.selectedTarget = null;
        battleManager.selectedParts.Clear();
        battleManager.UpdateTargetIndicator();
        battleManager.chatAI.HideInputUI();
        battleManager.turnIndicatorUI?.Hide();
    }

    #endregion

    #region Enemy

    public void EnemyAttack(Character enemy, Character target, CharacterActionData chosenAction)
    {
        battleManager.StartCoroutine(battleManager.chatAI.SendEnemyMessage(enemy, target, chosenAction));
    }

    public IEnumerator ResolveEnemyAttack(Character enemy, Character target, CharacterActionData chosenAction, float feasibility, float potential, string effectValue, string effectDesc)
    {
        if (enemy == null || target == null || !target.IsAlive())
        {
            Debug.LogWarning("Invalid enemy or target.");
            yield break;
        }

        float baseDamage = enemy.attack;
        DamageResult result = battleManager.damageCalculator.CalculateDamageNoCreativity(feasibility, potential, baseDamage, enemy, target);
        int calculatedDamage = Mathf.RoundToInt(result.damage);
        int finalDamage = calculatedDamage;

        foreach (var behavior in enemy.runtimePassiveBehaviors)
        {
            if (behavior is IDamageReaction reaction)
                reaction.OnBeforeDamage(enemy, target, ref finalDamage);
        }

        if (target is Player playerTarget)
        {
            foreach (var behavior in playerTarget.runtimePassiveBehaviors)
            {
                if (behavior is IDamageReaction reaction)
                    reaction.OnBeforeDamage(enemy, playerTarget, ref finalDamage);
            }

            foreach (var part in playerTarget.bodyParts)
            {
                var armor = part.equippedArmor;
                if (armor == null || armor.itemBehaviorPrefab == null) continue;

                var reaction = armor.itemBehaviorPrefab.GetComponent<IDamageReaction>();
                if (reaction != null)
                    reaction.OnBeforeDamage(enemy, playerTarget, ref finalDamage);
            }
        }

        enemy.pendingDamage = finalDamage;
        enemy.damageTarget = target;
        enemy.currentHitIndex = 0;
        enemy.selectedAction = chosenAction;

        yield return battleManager.WaitForAnimation(enemy, chosenAction.animationTrigger);

        yield return new WaitForSeconds(0.75f); // 🔸 Delay after animation

        if (target is Player playerAfter)
        {
            foreach (var behavior in playerAfter.runtimePassiveBehaviors)
            {
                if (behavior is IDamageReaction reaction)
                    reaction.OnAfterDamage(enemy, playerAfter, finalDamage);
            }

            foreach (var part in playerAfter.bodyParts)
            {
                var armor = part.equippedArmor;
                if (armor == null || armor.itemBehaviorPrefab == null) continue;

                var reaction = armor.itemBehaviorPrefab.GetComponent<IDamageReaction>();
                if (reaction != null)
                    reaction.OnAfterDamage(enemy, playerAfter, finalDamage);
            }
        }

        foreach (var behavior in enemy.runtimePassiveBehaviors)
        {
            if (behavior is IDamageReaction reaction)
                reaction.OnAfterDamage(enemy, target, finalDamage);
        }

        yield return new WaitForSeconds(0.25f); // 🔸 Short pause before log

        string log = $"Turn {battleManager.turnCount}: {enemy.characterName} used {chosenAction.actionName}  → Target: {target.characterName} Result: {target.currentHP} / {target.maxHP} ({battleManager.chatAI.baseEffect})";
        battleManager.battleLog.Add(log);
        Debug.Log(log);

        yield return new WaitForSeconds(0.75f); // 🔸 Let log register

        yield return battleManager.StartCoroutine(EndEnemyTurn());
    }

    public IEnumerator EndEnemyTurn()
    {
        yield return new WaitForSeconds(0.5f); // 🔸 Optional cooldown

        if (battleManager.CheckBattleEnd())
        {
            battleManager.battleActive = false;
            Debug.Log("Battle Finished!");
        }

        Debug.Log("EndEnemyTurn");
        battleManager.currentActingCharacter?.ClearEndTurnEffects();
        battleManager.turnCount++;
        battleManager.isActionPhase = false;
        battleManager.currentActingCharacter = null;
        battleManager.turnIndicatorUI?.Hide();
    }

    #endregion

    public string TryApplyStatusEffects(Character attacker, Character target)
    {
        List<string> appliedEffects = new();

        ApplyWeaponStatusChances(attacker);

        foreach (var behavior in attacker.runtimePassiveBehaviors)
        {
            if (behavior is IPossibilityModifier mod)
            {
                mod.ModifyChances(attacker.possibilityPool);
            }
        }

        bool forceCrit = attacker.runtimePassiveBehaviors.OfType<BloodRushCore>().Any(brc => brc.IsReady());

        attacker.isCritical = forceCrit || attacker.possibilityPool.Roll(StatusChanceType.Critical);

        if (attacker.isCritical)
            appliedEffects.Add($"landed a CRITICAL HIT on {target.characterName}");

        if (attacker.possibilityPool.Roll(StatusChanceType.Stun))
        {
            TurnStatusEffect stun = new(StatusEffectType.Stun, 1, 0, attacker);
            target.ApplyStatusEffect(stun);
            appliedEffects.Add($"stunned {target.characterName}");
        }

        if (target.characterType != CharacterType.Android && attacker.possibilityPool.Roll(StatusChanceType.Bleed))
        {
            int duration = target.characterType == CharacterType.Human ? 2 : 1;
            TurnStatusEffect bleed = new(StatusEffectType.Bleed, duration, 1, attacker);
            target.ApplyStatusEffect(bleed);
            appliedEffects.Add($"inflicted Bleed on {target.characterName} for {duration} turn(s)");
        }

        if (target.characterType != CharacterType.Android && attacker.possibilityPool.Roll(StatusChanceType.Poison))
        {
            TurnStatusEffect poison = new(StatusEffectType.Poison, 3, 1, attacker);
            target.ApplyStatusEffect(poison);
            appliedEffects.Add($"inflicted Poison on {target.characterName}");
        }

        return appliedEffects.Count > 0
            ? " Effects: " + string.Join(". ", appliedEffects) + "."
            : " No special effects.";
    }

    private void ApplyWeaponStatusChances(Character attacker)
    {
        float bleed = 0f, poison = 0f, stun = 0f, crit = 0f;

        foreach (var item in attacker.activeItem)
        {
            if (item is Weapon w)
            {
                bleed += w.bleedChance;
                poison += w.poisonChance;
                stun += w.stunChance;
                crit += w.criticalChance;
            }
        }

        if (attacker.selectedAction != null)
        {
            bleed += attacker.selectedAction.bleedChance;
            poison += attacker.selectedAction.poisonChance;
            stun += attacker.selectedAction.stunChance;
            crit += attacker.selectedAction.criticalChance;
        }

        if (attacker is Enemy)
        {
            var nodeType = PlayerData.Instance != null ? PlayerData.Instance.nextNodeType : Map.NodeType.MinorEnemy;
            if (nodeType == Map.NodeType.MinorEnemy) crit += 0.05f;
            else if (nodeType == Map.NodeType.EliteEnemy) crit += 0.1f;
        }

        attacker.possibilityPool.SetBaseChance(StatusChanceType.Bleed, bleed);
        attacker.possibilityPool.SetBaseChance(StatusChanceType.Poison, poison);
        attacker.possibilityPool.SetBaseChance(StatusChanceType.Stun, stun);
        attacker.possibilityPool.SetBaseChance(StatusChanceType.Critical, crit);
    }

    public void SaveLastDamageBreakdown(Character attacker, Dictionary<DamageType, float> breakdown)
    {
        lastDamageBreakdown[attacker] = breakdown;
    }

    public Dictionary<DamageType, float> GetLastDamageBreakdown(Character attacker)
    {
        return lastDamageBreakdown.TryGetValue(attacker, out var val) ? val : new();
    }
}
