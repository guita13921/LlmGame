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

        Debug.Log($"CreativityBonus: {creativityBonus} (Unique: {nUniqueWords}, Repeats: {nWordsUsedAtLeast2Times})");

        return creativityBonus;
    }

    public Dictionary<DamageType, int> GetActiveWeaponDamageBreakdown(Character character)
    {
        Dictionary<DamageType, int> damageBreakdown = new Dictionary<DamageType, int>();

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
                        DamageType.Digital => weapon.damageDigital,
                        DamageType.Plasma => weapon.damagePlasma,
                        DamageType.Laser => weapon.damageLaser,
                        DamageType.Chemical => weapon.damageChemical,
                        DamageType.Viral => weapon.damageViral,
                        _ => 0
                    };

                    if (!damageBreakdown.ContainsKey(dt))
                        damageBreakdown[dt] = value;
                    else
                        damageBreakdown[dt] += value;
                }
            }
        }

        return damageBreakdown;
    }

    public float CalculateDamage(
        float feasibility, float potential, float baseDamage,
        string userMessage, Character attacker, Character target)
    {
        const float constant = 1f;

        var weaponDamageBreakdown = GetActiveWeaponDamageBreakdown(attacker);
        float totalWeaponDamage = weaponDamageBreakdown.Values.Sum();
        float totalBaseDamage = baseDamage + totalWeaponDamage;

        Debug.Log("totalBaseDamage : " + totalBaseDamage);

        // 1. Gather LLM influence from body parts (still additive)
        float feasibilityModifierSum = 0f;
        float potentialModifierSum = 0f;

        var selectedParts = battleManager.selectedParts;
        if (selectedParts == null || selectedParts.Count == 0)
        {
            Debug.LogWarning("⚠️ No selected body parts.");
            return 0f;
        }

        // 2. Add income modifiers from exposed weak points on selected target parts
        foreach (var part in selectedParts)
        {
            if (part == null) continue;

            feasibilityModifierSum += part.feasibilityModifier;
            potentialModifierSum += part.potentialModifier;

            var wp = part.linkedWeakPoint;
            if (wp != null && wp.isExposed)
            {
                feasibilityModifierSum += wp.income_feasibilityModifier;
                potentialModifierSum += wp.income_potentialModifier;
                Debug.Log($"🛡️ [Income Weakness] {wp.weakPointName}: +F{wp.income_feasibilityModifier}, +P{wp.income_potentialModifier}");
            }
        }

        // 3. Add outcome modifiers from exposed weak points on selected attacker parts (if applicable)
        // NOTE: If you want to define a separate selectedParts list for attacker, replace `selectedParts` below
        foreach (var part in selectedParts)
        {
            if (part == null) continue;

            var wp = part.linkedWeakPoint;
            if (wp != null && wp.isExposed)
            {
                feasibilityModifierSum += wp.outcome_feasibilityModifier;
                potentialModifierSum += wp.outcome_potentialModifier;
                Debug.Log($"⚔️ [Outcome Weakness] {wp.weakPointName}: +F{wp.outcome_feasibilityModifier}, +P{wp.outcome_potentialModifier}");
            }
        }

        float finalFeasibility = Mathf.Max(0f, feasibility + feasibilityModifierSum);
        float finalPotential = Mathf.Max(0f, potential + potentialModifierSum);

        // 4. Apply Armor Reductions ONLY to selected target parts
        foreach (var part in selectedParts)
        {
            var armor = part.equippedArmor;
            if (armor == null) continue;

            finalFeasibility *= 1f - Mathf.Clamp01(armor.reduceFeasibility);
            finalPotential *= 1f - Mathf.Clamp01(armor.reducePotentialDamage);
        }

        // 5. LLM Scaling
        float llmDamageModifier = ((finalFeasibility / 10f) * (finalPotential / 10f)) * constant;
        float creativityBonus = CalculateCreativityBonus(userMessage, attacker);
        float llmScaledBaseDamage = totalBaseDamage * (1 + creativityBonus) * llmDamageModifier;

        Debug.Log($"[Damage] finalFeasibility (w/ creativity): {finalFeasibility}");
        Debug.Log($"[Damage] finalPotential (w/ creativity): {finalPotential}");
        Debug.Log($"[Damage] llmDamageModifier (w/ creativity): {llmDamageModifier}");
        Debug.Log($"[Damage] llmScaledBaseDamage (w/ creativity): {llmScaledBaseDamage}");

        // 6. Reduce Per-Type via Armor (only selected parts)
        var reducedDamageBreakdown = new Dictionary<DamageType, float>();
        foreach (var kvp in weaponDamageBreakdown)
        {
            float reduction = 0f;
            foreach (var part in selectedParts)
            {
                ArmorData armor = part.equippedArmor;
                if (armor == null) continue;

                reduction += kvp.Key switch
                {
                    DamageType.Physical => armor.reduceDamagePhysical,
                    DamageType.Fire => armor.reduceDamageFire,
                    DamageType.Electric => armor.reduceDamageElectric,
                    DamageType.Radiation => armor.reduceDamageRadiation,
                    DamageType.Explosive => armor.reduceDamageExplosive,
                    DamageType.Digital => armor.reduceDamageDigital,
                    DamageType.Plasma => armor.reduceDamagePlasma,
                    DamageType.Laser => armor.reduceDamageLaser,
                    DamageType.Chemical => armor.reduceDamageChemical,
                    DamageType.Viral => armor.reduceDamageViral,
                    _ => 0f
                };
            }

            float reduced = Mathf.Max(0f, kvp.Value - reduction);
            reducedDamageBreakdown[kvp.Key] = reduced;
        }

        float finalDamage = reducedDamageBreakdown.Values.Sum() + baseDamage;
        float scaledFinalDamage = Mathf.Max(0f, finalDamage * llmDamageModifier);

        Debug.Log($"[Final Damage]: {scaledFinalDamage}");

        // 7. Apply split damage to selected body parts
        float splitDamage = scaledFinalDamage / selectedParts.Count;
        foreach (var part in selectedParts)
        {
            part.ApplyDamage(Mathf.RoundToInt(splitDamage));
        }

        return scaledFinalDamage;
    }


    public float CalculateDamageNoCreativity(
        float feasibility, float potential, float baseDamage,
        Character attacker, Character target)
    {
        const float constant = 2f;

        // 1. Gather Damage Types and Values
        Dictionary<DamageType, int> weaponDamageBreakdown = new();
        float totalWeaponDamage = 0f;

        // ✅ If attacker has an active skill
        if (attacker.currentSkill is DamageModifierSkill skill)
        {
            // Use skill-specific damage types and values
            weaponDamageBreakdown = new Dictionary<DamageType, int>
        {
            { DamageType.Physical, skill.damagePhysical },
            { DamageType.Fire, skill.damageFire },
            { DamageType.Electric, skill.damageElectric },
            { DamageType.Radiation, skill.damageRadiation },
            { DamageType.Explosive, skill.damageExplosive },
            { DamageType.Digital, skill.damageDigital },
            { DamageType.Plasma, skill.damagePlasma },
            { DamageType.Laser, skill.damageLaser },
            { DamageType.Chemical, skill.damageChemical },
            { DamageType.Viral, skill.damageViral }
        };

            // Remove zero entries
            weaponDamageBreakdown = weaponDamageBreakdown
                .Where(kvp => kvp.Value > 0)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            totalWeaponDamage = weaponDamageBreakdown.Values.Sum();

            Debug.Log($"[Skill] {skill.skillName} damage breakdown: " +
                      $"{string.Join(", ", weaponDamageBreakdown.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}");
        }
        else
        {
            // Fallback to weapons
            weaponDamageBreakdown = GetActiveWeaponDamageBreakdown(attacker);
            totalWeaponDamage = weaponDamageBreakdown.Values.Sum();
        }

        float totalBaseDamage = baseDamage + totalWeaponDamage;

        Debug.Log("totalBaseDamage : " + totalBaseDamage);

        // 2. Feasibility & Potential Modifiers (based on selected parts)
        float feasibilityModifierSum = 0f;
        float potentialModifierSum = 0f;

        var selectedParts = battleManager.selectedParts;
        if (selectedParts == null || selectedParts.Count == 0)
        {
            Debug.LogWarning("⚠️ No selected body parts to apply damage.");
            return 0f;
        }

        foreach (var part in selectedParts)
        {
            if (part == null) continue;

            feasibilityModifierSum += part.feasibilityModifier;
            potentialModifierSum += part.potentialModifier;

            if (part.linkedWeakPoint != null && part.linkedWeakPoint.isExposed)
            {
                var wp = part.linkedWeakPoint;

                feasibilityModifierSum += wp.income_feasibilityModifier + wp.outcome_feasibilityModifier;
                potentialModifierSum += wp.income_potentialModifier + wp.outcome_potentialModifier;

                Debug.Log($"🧠 [WeakPoint] {wp.weakPointName} exposed: +F({wp.income_feasibilityModifier + wp.outcome_feasibilityModifier}), +P({wp.income_potentialModifier + wp.outcome_potentialModifier})");
            }
        }

        float finalFeasibility = Mathf.Max(0f, feasibility + feasibilityModifierSum);
        float finalPotential = Mathf.Max(0f, potential + potentialModifierSum);

        // 3. Apply Armor Reductions (feasibility/potential)
        foreach (var part in selectedParts)
        {
            ArmorData armor = part.equippedArmor;
            if (armor == null) continue;

            finalFeasibility *= 1f - Mathf.Clamp01(armor.reduceFeasibility);
            finalPotential *= 1f - Mathf.Clamp01(armor.reducePotentialDamage);
        }

        // 4. LLM Scaling
        float llmModifier = ((finalFeasibility / 10f) * (finalPotential / 10f)) * constant;
        float scaledLLMBaseDamage = totalBaseDamage * llmModifier;

        Debug.Log($"[LLM] Feasibility: {finalFeasibility}, Potential: {finalPotential}, ScaledBaseDamage: {scaledLLMBaseDamage}");

        // 5. Reduce Per-Damage-Type by Armor
        var reducedDamageBreakdown = new Dictionary<DamageType, float>();
        foreach (var kvp in weaponDamageBreakdown)
        {
            DamageType dt = kvp.Key;
            float original = kvp.Value;
            float reduction = 0f;

            foreach (var part in selectedParts)
            {
                ArmorData armor = part.equippedArmor;
                if (armor == null) continue;

                reduction += dt switch
                {
                    DamageType.Physical => armor.reduceDamagePhysical,
                    DamageType.Fire => armor.reduceDamageFire,
                    DamageType.Electric => armor.reduceDamageElectric,
                    DamageType.Radiation => armor.reduceDamageRadiation,
                    DamageType.Explosive => armor.reduceDamageExplosive,
                    DamageType.Digital => armor.reduceDamageDigital,
                    DamageType.Plasma => armor.reduceDamagePlasma,
                    DamageType.Laser => armor.reduceDamageLaser,
                    DamageType.Chemical => armor.reduceDamageChemical,
                    DamageType.Viral => armor.reduceDamageViral,
                    _ => 0f
                };
            }

            float reduced = Mathf.Max(0f, original - reduction);
            reducedDamageBreakdown[dt] = reduced;
            Debug.Log($"[Reduce] {dt}: -{reduction} => {reduced}");
        }

        // 6. Final Damage Calculation
        float finalDamage = reducedDamageBreakdown.Values.Sum() + baseDamage;
        float scaledFinalDamage = Mathf.Max(0f, finalDamage * llmModifier);

        Debug.Log($"[Final Damage] = {scaledFinalDamage}");

        // 7. Apply to Body Parts
        float damagePerPart = scaledFinalDamage / selectedParts.Count;
        foreach (var part in selectedParts)
        {
            part.ApplyDamage(Mathf.RoundToInt(damagePerPart));
        }

        return scaledFinalDamage;
    }

}

