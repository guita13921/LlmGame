using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;

public class DamageCalculator : MonoBehaviour
{
    private BattleManager battleManager;

    private void Awake()
    {
        battleManager = GetComponent<BattleManager>();
        if (battleManager == null)
            Debug.LogError("DamageCalculator requires BattleManager on the same GameObject.");
    }

    public float CalculateCreativityBonus(string userMessage, Character actor)
    {
        if (string.IsNullOrEmpty(userMessage)) return 0f;

        List<string> pastMessages = battleManager.GetPastMessagesFromActor(actor);
        pastMessages.Add(userMessage);

        string combinedText = string.Join(" ", pastMessages).ToLower();
        string[] words = combinedText.Split(new char[] {
            ' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}'
        }, System.StringSplitOptions.RemoveEmptyEntries);

        HashSet<string> uniqueWords = new HashSet<string>(words);
        int nUniqueWords = uniqueWords.Count;

        Dictionary<string, int> wordCount = new Dictionary<string, int>();
        foreach (string word in words)
        {
            if (wordCount.ContainsKey(word)) wordCount[word]++;
            else wordCount[word] = 1;
        }

        int nWordsUsedAtLeast2Times = wordCount.Count(kvp => kvp.Value >= 2);
        float uniqueWordBonus = Mathf.Min(1f, nUniqueWords * 0.05f);
        float repetitionPenalty = Mathf.Min(0.3f, nWordsUsedAtLeast2Times * 0.02f);

        float creativityBonus = uniqueWordBonus - repetitionPenalty;

        if (battleManager.showDebug)Debug.Log($"CreativityBonus: {creativityBonus} (Unique: {nUniqueWords}, Repeats: {nWordsUsedAtLeast2Times})");

        return creativityBonus;
    }

    public Dictionary<DamageType, int> GetActiveWeaponDamageBreakdown(Character character)
    {
        Dictionary<DamageType, int> damageBreakdown = new();

        // 🛠️ Allow crit-modifiers to apply first
        foreach (var behavior in character.runtimePassiveBehaviors)
        {
            if (behavior is IPossibilityModifier mod)
            {
                mod.ModifyCritical(character, character.possibilityPool); // 🧠 This must happen first
            }
        }

        float critMultiplier = character.isCritical
            ? character.possibilityPool.GetFinalCriticalMultiplier()
            : 1f;

        foreach (var activeItem in character.activeItem)
        {
            if (activeItem is Weapon weapon)
            {
                foreach (var dt in weapon.damageType)
                {
                    int value = dt switch
                    {
                        DamageType.Physical => weapon.damagePhysical,
                        DamageType.Fire => weapon.damageFire,
                        DamageType.Electric => weapon.damageElectric,
                        DamageType.Radiation => weapon.damageRadiation,
                        DamageType.Explosive => weapon.damageExplosive,
                        DamageType.Plasma => weapon.damagePlasma,
                        DamageType.Poison => weapon.damageChemical,
                        DamageType.Viral => weapon.damageViral,
                        _ => 0
                    };

                    value = Mathf.RoundToInt(value * critMultiplier);

                    if (!damageBreakdown.ContainsKey(dt))
                        damageBreakdown[dt] = value;
                    else
                        damageBreakdown[dt] += value;
                }
            }
        }

        return damageBreakdown;
    }

    public DamageResult CalculateDamage(
        float feasibility, float potential, float baseDamage,
        string userMessage, Character attacker, Character target)
    {
        const float constant = 1f;

        var weaponDamageBreakdown = GetActiveWeaponDamageBreakdown(attacker);
        float totalWeaponDamage = weaponDamageBreakdown.Values.Sum();
        float totalBaseDamage = baseDamage + totalWeaponDamage;

        Debug.Log("totalBaseDamage : " + totalBaseDamage);

        // 🧠 Modifiers from body parts and weak points
        float feasibilityModifierSum = 0f;
        float potentialModifierSum = 0f;

        var selectedTargetParts = battleManager.selectedParts;
        var attackerParts = attacker.bodyParts;

        if (selectedTargetParts == null || selectedTargetParts.Count == 0)
        {
            Debug.LogWarning("⚠️ No selected body parts on target.");
        }

        // 🛡️ 1. Income WeakPoints (from target)
        foreach (var part in selectedTargetParts)
        {
            if (part == null) continue;

            feasibilityModifierSum += part.feasibilityModifier;
            potentialModifierSum += part.potentialModifier;

            var wp = part.linkedWeakPoint;
            if (wp != null)
            {
                feasibilityModifierSum += wp.income_feasibilityModifier;
                potentialModifierSum += wp.income_potentialModifier;
                Debug.Log($"🛡️ [Target Income WeakPoint] {wp.weakPointName}: +F{wp.income_feasibilityModifier}, +P{wp.income_potentialModifier}");
            }
        }

        // ⚔️ 2. Outcome WeakPoints (from attacker)
        foreach (var part in attackerParts)
        {
            if (part == null) continue;

            var wp = part.linkedWeakPoint;
            if (wp != null)
            {
                feasibilityModifierSum += wp.outcome_feasibilityModifier;
                potentialModifierSum += wp.outcome_potentialModifier;
                Debug.Log($"⚔️ [Attacker Outcome WeakPoint] {wp.weakPointName}: +F{wp.outcome_feasibilityModifier}, +P{wp.outcome_potentialModifier}");
            }
        }

        // 🧮 3. Final Feasibility & Potential after modifiers
        float finalFeasibility = Mathf.Max(0f, feasibility + feasibilityModifierSum);
        float finalPotential = Mathf.Max(0f, potential + potentialModifierSum);

        // 🔍 Check for Ghostscope Overlay (ignore feasibility/potential reduction with ranged weapons)
        bool ignoreArmorPenalty = false;
        Weapon activeWeapon = attacker is Player ap ? ap.equippedWeapon : null;
        if (activeWeapon != null && activeWeapon.weaponType == WeaponType.Ranged_Weapon)
        {
            foreach (var part in attacker.bodyParts)
            {
                var armor = part.equippedArmor;
                if (armor != null && armor.itemBehaviorPrefab != null &&
                    armor.itemBehaviorPrefab.GetComponent<GhostscopeOverlay>() != null)
                {
                    ignoreArmorPenalty = true;
                    break;
                }
            }
        }

        // 🛡️ 4. Apply armor reduction (target only)
        foreach (var part in selectedTargetParts)
        {
            var armor = part.equippedArmor;
            if (armor == null) continue;

            if (!ignoreArmorPenalty)
            {
                finalFeasibility *= 1f - Mathf.Clamp01(armor.reduceFeasibility);
                finalPotential *= 1f - Mathf.Clamp01(armor.reducePotentialDamage);
            }
        }

        // 🧠 5. LLM Scaling
        float llmDamageModifier = ((finalFeasibility / 10f) * (finalPotential / 10f)) * constant;
        float creativityBonus = CalculateCreativityBonus(userMessage, attacker);
        float llmScaledBaseDamage = totalBaseDamage * (1 + creativityBonus) * llmDamageModifier;

        if (battleManager.showDebug)Debug.Log($"[Damage] finalFeasibility: {finalFeasibility}");
        if (battleManager.showDebug)Debug.Log($"[Damage] finalPotential: {finalPotential}");
        if (battleManager.showDebug)Debug.Log($"[Damage] llmModifier: {llmDamageModifier}");
        if (battleManager.showDebug)Debug.Log($"[Damage] llmScaledBaseDamage: {llmScaledBaseDamage}");

        // 🧪 6. Type-based armor reduction (on selected target parts)
        // 🧪 6. Type-based armor reduction (on selected target parts)
        var reducedDamageBreakdown = new Dictionary<DamageType, float>();

        foreach (var kvp in weaponDamageBreakdown)
        {
            float reduction = 0f;

            foreach (var part in selectedTargetParts)
            {
                ArmorData armor = part.equippedArmor;
                if (armor == null || armor.itemBehaviorPrefab == null) continue;

                // ✅ Skip if GhostscopeOverlay is equipped on this body part
                if (armor.itemBehaviorPrefab.GetComponent<GhostscopeOverlay>() != null)
                    continue;

                reduction += kvp.Key switch
                {
                    DamageType.Physical => armor.reduceDamagePhysical,
                    DamageType.Fire => armor.reduceDamageFire,
                    DamageType.Electric => armor.reduceDamageElectric,
                    DamageType.Radiation => armor.reduceDamageRadiation,
                    DamageType.Explosive => armor.reduceDamageExplosive,
                    DamageType.Plasma => armor.reduceDamagePlasma,
                    DamageType.Poison => armor.reduceDamageChemical,
                    DamageType.Viral => armor.reduceDamageViral,
                    _ => 0f
                };
            }

            float reduced = Mathf.Max(0f, kvp.Value - reduction);
            reducedDamageBreakdown[kvp.Key] = reduced;
        }


        // 🎯 7. Final scaled damage
        float finalDamage = reducedDamageBreakdown.Values.Sum() + baseDamage;
        float scaledFinalDamage = Mathf.Max(0f, finalDamage * llmDamageModifier);

        // 💥 8. Apply split damage to selected target parts (skip if shield active)
        if (target.currentshield <= 0)
        {
            float splitDamage = scaledFinalDamage / selectedTargetParts.Count;
            Weapon usedWeapon = attacker is Player player ? player.equippedWeapon : null;

            foreach (var part in selectedTargetParts)
            {
                int damageToApply = Mathf.RoundToInt(splitDamage);

                // 🔥 Apply OnHit Modifiers from Passive Items (like Head Targeting)
                if (attacker is Player attackerPlayer)
                {
                    foreach (var behavior in attackerPlayer.runtimePassiveBehaviors)
                    {
                        if (behavior is IHitModifier hitModifier)
                        {
                            hitModifier.OnHit(attacker, part, ref damageToApply);
                        }
                    }
                }

                // 💥 Apply damage to the body part (with modified damage)
                part.ApplyDamage(damageToApply, true, usedWeapon);
            }
        }


        Debug.Log($"[Final Damage]: {scaledFinalDamage}");

        // 💾 Store for AI display
        battleManager.chatAI.baseFeasibility = finalFeasibility;
        battleManager.chatAI.basePotential = finalPotential;

        return new DamageResult(scaledFinalDamage, finalFeasibility, finalPotential);
    }

    public DamageResult CalculateDamageNoCreativity(
        float feasibility, float potential, float baseDamage,
        Character attacker, Character target)
    {
        const float constant = 1.0f;

        // 1️⃣ Gather Damage from skill or weapon
        Dictionary<DamageType, int> weaponDamageBreakdown = new();
        float totalWeaponDamage = 0f;

        if (attacker.currentSkill is DamageModifierSkill skill)
        {
            weaponDamageBreakdown = new Dictionary<DamageType, int>
        {
            { DamageType.Physical, skill.damagePhysical },
            { DamageType.Fire, skill.damageFire },
            { DamageType.Electric, skill.damageElectric },
            { DamageType.Radiation, skill.damageRadiation },
            { DamageType.Explosive, skill.damageExplosive },
            { DamageType.Plasma, skill.damagePlasma },
            { DamageType.Poison, skill.damageChemical },
            { DamageType.Viral, skill.damageViral }
        };

            weaponDamageBreakdown = weaponDamageBreakdown
                .Where(kvp => kvp.Value > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            totalWeaponDamage = weaponDamageBreakdown.Values.Sum();

            Debug.Log($"[Skill] {skill.skillName} damage breakdown: " +
                      $"{string.Join(", ", weaponDamageBreakdown.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
        }
        else
        {
            weaponDamageBreakdown = GetActiveWeaponDamageBreakdown(attacker);
            totalWeaponDamage = weaponDamageBreakdown.Values.Sum();
        }

        float totalBaseDamage = baseDamage + totalWeaponDamage;
        Debug.Log("totalBaseDamage : " + totalBaseDamage);

        // 2️⃣ Calculate modifier from weak points
        float feasibilityModifierSum = 0f;
        float potentialModifierSum = 0f;

        var selectedTargetParts = battleManager.selectedParts;
        var attackerParts = attacker.bodyParts;

        if (selectedTargetParts == null || selectedTargetParts.Count == 0)
        {
            Debug.LogWarning("⚠️ No selected body parts on target.");
        }

        // 🛡️ Income weak points from target
        foreach (var part in selectedTargetParts)
        {
            if (part == null) continue;

            feasibilityModifierSum += part.feasibilityModifier;
            potentialModifierSum += part.potentialModifier;

            var wp = part.linkedWeakPoint;
            if (wp != null)
            {
                feasibilityModifierSum += wp.income_feasibilityModifier;
                potentialModifierSum += wp.income_potentialModifier;
                Debug.Log($"🛡️ [Target Income WeakPoint] {wp.weakPointName}: +F{wp.income_feasibilityModifier}, +P{wp.income_potentialModifier}");
            }
        }

        // ⚔️ Outcome weak points from attacker
        foreach (var part in attackerParts)
        {
            if (part == null) continue;

            var wp = part.linkedWeakPoint;
            if (wp != null)
            {
                feasibilityModifierSum += wp.outcome_feasibilityModifier;
                potentialModifierSum += wp.outcome_potentialModifier;
                Debug.Log($"⚔️ [Attacker Outcome WeakPoint] {wp.weakPointName}: +F{wp.outcome_feasibilityModifier}, +P{wp.outcome_potentialModifier}");
            }
        }

        // 3️⃣ Final feasibility & potential after armor
        float finalFeasibility = Mathf.Max(0f, feasibility + feasibilityModifierSum);
        float finalPotential = Mathf.Max(0f, potential + potentialModifierSum);

        bool ignoreArmorPenalty = false;
        Weapon activeWeapon = attacker is Player ap ? ap.equippedWeapon : null;
        if (activeWeapon != null && activeWeapon.weaponType == WeaponType.Ranged_Weapon)
        {
            foreach (var part in attacker.bodyParts)
            {
                var armor = part.equippedArmor;
                if (armor != null && armor.itemBehaviorPrefab != null &&
                    armor.itemBehaviorPrefab.GetComponent<GhostscopeOverlay>() != null)
                {
                    ignoreArmorPenalty = true;
                    break;
                }
            }
        }

        foreach (var part in selectedTargetParts)
        {
            ArmorData armor = part.equippedArmor;
            if (armor == null) continue;

            if (!ignoreArmorPenalty)
            {
                finalFeasibility *= 1f - Mathf.Clamp01(armor.reduceFeasibility);
                finalPotential *= 1f - Mathf.Clamp01(armor.reducePotentialDamage);
            }
        }

        // 4️⃣ Final scaling
        float llmModifier = ((finalFeasibility / 10f) * (finalPotential / 10f)) * constant;
        float scaledLLMBaseDamage = totalBaseDamage * llmModifier;

        Debug.Log($"[LLM] Feasibility: {finalFeasibility}, Potential: {finalPotential}, ScaledBaseDamage: {scaledLLMBaseDamage}");

        // 5️⃣ Armor type reductions per damage type
        // 🧪 6. Type-based armor reduction (on selected target parts)
        var reducedDamageBreakdown = new Dictionary<DamageType, float>();

        foreach (var kvp in weaponDamageBreakdown)
        {
            float reduction = 0f;

            foreach (var part in selectedTargetParts)
            {
                ArmorData armor = part.equippedArmor;
                if (armor == null || armor.itemBehaviorPrefab == null) continue;

                // ✅ Skip if GhostscopeOverlay is equipped on this body part
                if (armor.itemBehaviorPrefab.GetComponent<GhostscopeOverlay>() != null)
                    continue;

                reduction += kvp.Key switch
                {
                    DamageType.Physical => armor.reduceDamagePhysical,
                    DamageType.Fire => armor.reduceDamageFire,
                    DamageType.Electric => armor.reduceDamageElectric,
                    DamageType.Radiation => armor.reduceDamageRadiation,
                    DamageType.Explosive => armor.reduceDamageExplosive,
                    DamageType.Plasma => armor.reduceDamagePlasma,
                    DamageType.Poison => armor.reduceDamageChemical,
                    DamageType.Viral => armor.reduceDamageViral,
                    _ => 0f
                };
            }

            float reduced = Mathf.Max(0f, kvp.Value - reduction);
            reducedDamageBreakdown[kvp.Key] = reduced;
        }

        battleManager.combatHandler.SaveLastDamageBreakdown(attacker, reducedDamageBreakdown);

        // 6️⃣ Final damage value
        float finalDamage = reducedDamageBreakdown.Values.Sum() + baseDamage;
        float scaledFinalDamage = Mathf.Max(0f, finalDamage * llmModifier);

        Debug.Log($"[Final Damage] = {scaledFinalDamage}");

        // 7️⃣ Apply to body parts (skip if shield active)
        if (target.currentshield <= 0)
        {
            float damagePerPart = scaledFinalDamage / selectedTargetParts.Count;
            Weapon weapon = attacker is Player player ? player.equippedWeapon : null;

            if (attacker.currentSkill != null && attacker.currentSkill.isDamagePercentagePart)
            {
                float percent = attacker.currentSkill.percentDamgePerPart / 100f;

                foreach (var part in selectedTargetParts)
                {
                    int damageToApply = Mathf.RoundToInt(damagePerPart);

                    // 🔥 Apply OnHit Modifiers
                    if (attacker is Player attackerPlayer)
                    {
                        foreach (var behavior in attackerPlayer.runtimePassiveBehaviors)
                        {
                            if (behavior is IHitModifier hitModifier)
                            {
                                hitModifier.OnHit(attacker, part, ref damageToApply);
                            }
                        }
                    }

                    part.ApplyDamage(damageToApply, true, weapon);
                }

            }
            else
            {
                foreach (var part in selectedTargetParts)
                {
                    int damageToApply = Mathf.RoundToInt(damagePerPart);

                    // 🔥 Apply OnHit Modifiers
                    if (attacker is Player attackerPlayer)
                    {
                        foreach (var behavior in attackerPlayer.runtimePassiveBehaviors)
                        {
                            if (behavior is IHitModifier hitModifier)
                            {
                                hitModifier.OnHit(attacker, part, ref damageToApply);
                            }
                        }
                    }

                    part.ApplyDamage(damageToApply, true, weapon);
                }

            }
        }

        // 8️⃣ Store LLM values for UI
        battleManager.chatAI.baseFeasibility = finalFeasibility;
        battleManager.chatAI.basePotential = finalPotential;

        return new DamageResult(scaledFinalDamage, finalFeasibility, finalPotential);
    }

}