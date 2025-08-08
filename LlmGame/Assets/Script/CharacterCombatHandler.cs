using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using System;

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
        var player = battleManager.currentActingCharacter as Player;
        if (player == null || target == null || !target.IsAlive())
        {
            Debug.LogWarning("Invalid player or target.");
            yield break;
        }

        float baseDamage = player.attack;
        int calculatedDamage;

        // ✅ Damage Calculation
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

        // ================================
        // ✅ BEFORE DAMAGE MODIFIERS
        // ================================

        int finalDamage = calculatedDamage;

        // Attacker passives & items
        foreach (var behavior in player.runtimePassiveBehaviors)
        {
            if (behavior is IDamageReaction reaction)
                reaction.OnBeforeDamage(player, target, ref finalDamage);
        }

        foreach (var item in player.equippedPassiveItems)
        {
            if (item.itemPrefab == null) continue;
            var reaction = item.itemPrefab.GetComponent<IDamageReaction>();
            if (reaction != null)
                reaction.OnBeforeDamage(player, target, ref finalDamage);
        }

        // Target passives, items, armor
        if (target is Player targetPlayer)
        {
            foreach (var behavior in targetPlayer.runtimePassiveBehaviors)
            {
                if (behavior is IDamageReaction reaction)
                    reaction.OnBeforeDamage(player, targetPlayer, ref finalDamage);
            }

            foreach (var item in targetPlayer.equippedPassiveItems)
            {
                if (item.itemPrefab == null) continue;
                var reaction = item.itemPrefab.GetComponent<IDamageReaction>();
                if (reaction != null)
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

        // ✅ Assign for use in animation event
        player.pendingDamage = finalDamage;
        player.damageTarget = target;
        player.currentHitIndex = 0;
        player.selectedAction = chosenAction;

        // ✅ Play animation (calls ApplyDamageAtHit which now just applies `pendingDamage`)
        if (player.isUsingUltimateSkill)
        {
            yield return battleManager.WaitForAnimation(player, battleManager.player.currentSkill.aniamtionTrigger);
        }
        else
        {
            yield return battleManager.WaitForAnimation(player, chosenAction.animationTrigger);
        }

        // ✅ AFTER DAMAGE REACTIONS
        if (target is Player targetAfter)
        {
            // Defender reactions
            foreach (var behavior in targetAfter.runtimePassiveBehaviors)
            {
                if (behavior is IDamageReaction reaction)
                    reaction.OnAfterDamage(player, targetAfter, finalDamage);
            }

            foreach (var item in targetAfter.equippedPassiveItems)
            {
                if (item.itemPrefab == null) continue;
                var reaction = item.itemPrefab.GetComponent<IDamageReaction>();
                if (reaction != null)
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

        // Attacker reactions
        foreach (var behavior in player.runtimePassiveBehaviors)
        {
            if (behavior is IDamageReaction reaction)
                reaction.OnAfterDamage(player, target, finalDamage);
        }

        foreach (var item in player.equippedPassiveItems)
        {
            if (item.itemPrefab == null) continue;
            var reaction = item.itemPrefab.GetComponent<IDamageReaction>();
            if (reaction != null)
                reaction.OnAfterDamage(player, target, finalDamage);
        }

        // ✅ Log
        //string log = $"Turn {battleManager.turnCount}: {player.characterName} {battleManager.playerInputField.text}  → Target: {target.characterName} Result: {target.currentHP} / {target.maxHP} ({battleManager.chatAI.baseEffect})";
        //battleManager.battleLog.Add(log);
        //Debug.Log(log);
        //Debug.Log(target.GetBodyPartStatus());

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

    public IEnumerator ResolveEnemyAttack(Character enemy, Character target, CharacterActionData chosenAction, float feasibility, float potential, string effectValue, string effectDesc)
    {
        if (enemy == null || target == null || !target.IsAlive())
        {
            Debug.LogWarning("Invalid enemy or target.");
            yield break;
        }

        // ======================================
        // 🔸 Step 1: Calculate Base Damage
        // ======================================
        float baseDamage = enemy.attack;
        DamageResult result = battleManager.damageCalculator.CalculateDamageNoCreativity(feasibility, potential, baseDamage, enemy, target);
        int calculatedDamage = Mathf.RoundToInt(result.damage);
        int finalDamage = calculatedDamage;

        // ======================================
        // 🔸 Step 2: Apply OnBeforeDamage
        // ======================================

        // Attacker passives
        foreach (var behavior in enemy.runtimePassiveBehaviors)
        {
            if (behavior is IDamageReaction reaction)
                reaction.OnBeforeDamage(enemy, target, ref finalDamage);
        }

        // Target passives
        if (target is Player playerTarget)
        {
            foreach (var behavior in playerTarget.runtimePassiveBehaviors)
            {
                if (behavior is IDamageReaction reaction)
                    reaction.OnBeforeDamage(enemy, playerTarget, ref finalDamage);
            }

            // Equipped passive items
            foreach (var item in playerTarget.equippedPassiveItems)
            {
                if (item.itemPrefab == null) continue;
                var reaction = item.itemPrefab.GetComponent<IDamageReaction>();
                if (reaction != null)
                    reaction.OnBeforeDamage(enemy, playerTarget, ref finalDamage);
            }

            // Armor on body parts
            foreach (var part in playerTarget.bodyParts)
            {
                var armor = part.equippedArmor;
                if (armor == null || armor.itemBehaviorPrefab == null) continue;

                var reaction = armor.itemBehaviorPrefab.GetComponent<IDamageReaction>();
                if (reaction != null)
                    reaction.OnBeforeDamage(enemy, playerTarget, ref finalDamage);
            }
        }

        // ======================================
        // 🔸 Step 3: Assign Final Damage for Animation
        // ======================================
        enemy.pendingDamage = finalDamage;
        enemy.damageTarget = target;
        enemy.currentHitIndex = 0;
        enemy.selectedAction = chosenAction;

        // ======================================
        // 🔸 Step 4: Play Animation → Triggers ApplyDamageAtHit()
        // ======================================
        yield return battleManager.WaitForAnimation(enemy, chosenAction.animationTrigger);

        // ======================================
        // 🔸 Step 5: Apply OnAfterDamage
        // ======================================

        // Target reactions (Player)
        if (target is Player playerAfter)
        {
            foreach (var behavior in playerAfter.runtimePassiveBehaviors)
            {
                if (behavior is IDamageReaction reaction)
                    reaction.OnAfterDamage(enemy, playerAfter, finalDamage);
            }

            foreach (var item in playerAfter.equippedPassiveItems)
            {
                if (item.itemPrefab == null) continue;
                var reaction = item.itemPrefab.GetComponent<IDamageReaction>();
                if (reaction != null)
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

        // Attacker post-attack reactions
        foreach (var behavior in enemy.runtimePassiveBehaviors)
        {
            if (behavior is IDamageReaction reaction)
                reaction.OnAfterDamage(enemy, target, finalDamage);
        }

        // ======================================
        // 🔸 Step 6: Log
        // ======================================
        string log = $"Turn {battleManager.turnCount}: {enemy.characterName} used {chosenAction.actionName}  → Target: {target.characterName} Result: {target.currentHP} / {target.maxHP} ({battleManager.chatAI.baseEffect})";
        battleManager.battleLog.Add(log);
        Debug.Log(log);

        // ======================================
        // 🔸 Step 7: End Turn
        // ======================================
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

        // 🔄 Refresh base chances from the attacker's active weapon(s)
        ApplyWeaponStatusChances(attacker);

        // 🧠 Check if any passive guarantees crit (like BloodRushCore)
        bool forceCrit = false;

        foreach (var behavior in attacker.runtimePassiveBehaviors)
        {
            if (behavior is BloodRushCore brc && brc.IsReady())
            {
                forceCrit = true;

                // ❌ Don’t consume here!
                // brc.Consume(); ← REMOVE THIS
                break;
            }
        }

        attacker.isCritical = forceCrit || attacker.possibilityPool.Roll(StatusChanceType.Critical);
        bool isCriticalHit = forceCrit || attacker.possibilityPool.Roll(StatusChanceType.Critical);

        if (isCriticalHit)
        {
            appliedEffects.Add($"landed a CRITICAL HIT on {target.characterName}");
            attacker.isCritical = true;  // 🔥 Mark attacker for damage calculation
        }
        else
        {
            attacker.isCritical = false;
        }

        // ⚡ Stun chance roll
        if (attacker.possibilityPool.Roll(StatusChanceType.Stun))
        {
            TurnStatusEffect stun = new TurnStatusEffect(StatusEffectType.Stun, 1, 0, attacker);
            target.ApplyStatusEffect(stun);
            appliedEffects.Add($"stunned {target.characterName}");
        }

        // 🩸 Bleed chance roll
        if (target.characterType != CharacterType.Android && attacker.possibilityPool.Roll(StatusChanceType.Bleed))
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
        if (target.characterType != CharacterType.Android && attacker.possibilityPool.Roll(StatusChanceType.Poison))
        {
            TurnStatusEffect poison = new TurnStatusEffect(StatusEffectType.Poison, 3, 1, attacker);
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

    /// <summary>
    /// Updates the attacker's possibility pool using status effect chances
    /// provided by any active weapons. Multiple weapons stack additively and
    /// values are clamped between 0 and 1 by the pool itself.
    /// </summary>
    /// <param name="attacker">Character performing the attack.</param>
    private void ApplyWeaponStatusChances(Character attacker)
    {
        float bleed = 0f;
        float poison = 0f;
        float stun = 0f;
        float crit = 0f;

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
