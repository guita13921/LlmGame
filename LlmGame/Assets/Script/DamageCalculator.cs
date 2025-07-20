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

        // 1. Gather LLM influence from body parts (still additive)
        float feasibilityModifierSum = 0f;
        float potentialModifierSum = 0f;

        foreach (var part in target.bodyParts)
        {
            if (part == null) continue;
            feasibilityModifierSum += part.feasibilityModifier;
            potentialModifierSum += part.potentialModifier;
        }

        // 2. Add income modifiers from exposed weak points on target
        foreach (var part in target.bodyParts)
        {
            var wp = part.linkedWeakPoint;
            if (wp != null && wp.isExposed)
            {
                feasibilityModifierSum += wp.income_feasibilityModifier;
                potentialModifierSum += wp.income_potentialModifier;
                Debug.Log($"🛡️ [Income Weakness] {wp.weakPointName}: +F{wp.income_feasibilityModifier}, +P{wp.income_potentialModifier}");
            }
        }

        // 3. Add outcome modifiers from exposed weak points on attacker
        foreach (var part in attacker.bodyParts)
        {
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

        // 4. Apply Armor Reductions
        foreach (var part in target.bodyParts)
        {
            var armor = part.equippedArmor;
            if (armor == null) continue;

            finalFeasibility *= 1f - Mathf.Clamp01(armor.reduceFeasibility);
            finalPotential *= 1f - Mathf.Clamp01(armor.reducePotentialDamage);
        }

        // 5. LLM Scaling
        float llmDamageModifier = ((finalFeasibility / 10f) * (finalPotential / 10f)) * constant;
        float llmScaledBaseDamage = totalBaseDamage * (1 + CalculateCreativityBonus(userMessage, attacker)) * llmDamageModifier;

        Debug.Log($"[Damage] Scaled (w/ creativity): {llmScaledBaseDamage}");

        // 6. Reduce Per-Type via Armor
        var reducedDamageBreakdown = new Dictionary<DamageType, float>();
        foreach (var kvp in weaponDamageBreakdown)
        {
            float reduction = 0f;
            foreach (var part in target.bodyParts)
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

        var targetParts = battleManager.selectedParts;
        if (targetParts == null || targetParts.Count == 0)
        {
            Debug.LogWarning("⚠️ No selected body parts.");
            return scaledFinalDamage;
        }

        float splitDamage = scaledFinalDamage / targetParts.Count;
        foreach (var part in targetParts)
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

        // 1. Weapon Damage Breakdown
        var weaponDamageBreakdown = GetActiveWeaponDamageBreakdown(attacker);
        float totalWeaponDamage = weaponDamageBreakdown.Values.Sum();
        float totalBaseDamage = baseDamage + totalWeaponDamage;

        // 2. Feasibility & Potential Modifiers
        float feasibilityModifierSum = 0f;
        float potentialModifierSum = 0f;

        // → From target body parts
        foreach (var part in target.bodyParts)
        {
            if (part == null) continue;

            feasibilityModifierSum += part.feasibilityModifier;
            potentialModifierSum += part.potentialModifier;
        }

        // → From target's exposed WeakPoints (income modifiers)
        foreach (var part in target.bodyParts)
        {
            var wp = part.linkedWeakPoint;
            if (wp != null && wp.isExposed)
            {
                feasibilityModifierSum += wp.income_feasibilityModifier;
                potentialModifierSum += wp.income_potentialModifier;
                Debug.Log($"🛡️ [Income Weakness] {wp.weakPointName}: +F{wp.income_feasibilityModifier}, +P{wp.income_potentialModifier}");
            }
        }

        // → From attacker's exposed WeakPoints (outcome modifiers)
        foreach (var part in attacker.bodyParts)
        {
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

        // 3. Apply Armor Reductions (multiplicative)
        foreach (var part in target.bodyParts)
        {
            ArmorData armor = part.equippedArmor;
            if (armor == null) continue;

            finalFeasibility *= 1f - Mathf.Clamp01(armor.reduceFeasibility);
            finalPotential *= 1f - Mathf.Clamp01(armor.reducePotentialDamage);
        }

        // 4. LLM Scaling
        float llmDamageModifier = ((finalFeasibility / 10f) * (finalPotential / 10f)) * constant;
        float llmScaledBaseDamage = totalBaseDamage * llmDamageModifier;

        Debug.Log($"[Enemy Damage Before Reduction]: {llmScaledBaseDamage}");

        // 5. Reduce Per-Damage-Type by Armor
        var reducedDamageBreakdown = new Dictionary<DamageType, float>();
        foreach (var kvp in weaponDamageBreakdown)
        {
            DamageType dt = kvp.Key;
            float original = kvp.Value;
            float totalReduction = 0f;

            foreach (var part in target.bodyParts)
            {
                ArmorData armor = part.equippedArmor;
                if (armor == null) continue;

                totalReduction += dt switch
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

            float reduced = Mathf.Max(0f, original - totalReduction);
            reducedDamageBreakdown[dt] = reduced;

            Debug.Log($"[Reduce] {dt}: -{totalReduction} => {reduced}");
        }

        // 6. Final Damage
        float finalDamage = reducedDamageBreakdown.Values.Sum() + baseDamage;
        float scaledFinalDamage = Mathf.Max(0f, finalDamage * llmDamageModifier);

        Debug.Log($"[Final Damage]: {scaledFinalDamage}");

        // 7. Apply to selected body parts
        List<BodyPartData> targetParts = battleManager.selectedParts;
        if (targetParts == null || targetParts.Count == 0)
        {
            Debug.LogWarning("⚠️ No selected body parts to apply damage.");
            return scaledFinalDamage;
        }

        float splitDamage = scaledFinalDamage / targetParts.Count;
        foreach (var part in targetParts)
        {
            part.ApplyDamage(Mathf.RoundToInt(splitDamage));
        }

        return scaledFinalDamage;
    }


}

