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

        // 🧠 Apply part-based feasibility/potential modifiers (additive)
        float feasibilityModifierSum = target.bodyParts.Sum(p => p.feasibilityModifier);
        float potentialModifierSum = target.bodyParts.Sum(p => p.potentialModifier);

        float finalFeasibility = Mathf.Max(0f, feasibility + feasibilityModifierSum);
        float finalPotential = Mathf.Max(0f, potential + potentialModifierSum);

        // 🛡️ Apply armor reductions (multiplicative)
        foreach (var part in target.bodyParts)
        {
            ArmorData armor = part.equippedArmor;
            if (armor == null) continue;

            finalFeasibility *= 1f - Mathf.Clamp01(armor.reduceFeasibility);
            finalPotential *= 1f - Mathf.Clamp01(armor.reducePotentialDamage);
        }

        float llmDamageModifier = ((finalFeasibility / 10f) * (finalPotential / 10f)) * constant;
        float llmScaledBaseDamage = totalBaseDamage * llmDamageModifier;

        float creativityBonus = CalculateCreativityBonus(userMessage, attacker);
        float damageBeforeReduction = llmScaledBaseDamage * (1 + creativityBonus);

        Debug.Log($"[Damage] Before Reduction: {damageBeforeReduction} (Feasibility: {finalFeasibility}, Potential: {finalPotential})");

        var reducedDamageBreakdown = new Dictionary<DamageType, float>();
        foreach (var kvp in weaponDamageBreakdown)
        {
            DamageType dt = kvp.Key;
            float typeDamage = kvp.Value;
            float reduction = 0f;

            foreach (var part in target.bodyParts)
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

            float reduced = Mathf.Max(0f, typeDamage - reduction);
            reducedDamageBreakdown[dt] = reduced;
            Debug.Log($"[Reduce] {dt}: -{reduction} => {reduced}");
        }

        float finalDamage = reducedDamageBreakdown.Values.Sum() + baseDamage;
        float scaledFinalDamage = Mathf.Max(0f, finalDamage * llmDamageModifier * (1 + creativityBonus));

        Debug.Log($"[Final Damage]: {scaledFinalDamage}");

        List<BodyPartData> targetParts = battleManager.selectedParts;

        if (targetParts == null || targetParts.Count == 0)
        {
            BodyPartData torso = target.bodyParts.FirstOrDefault(p => p.type == BodyPartType.Torso);
            if (torso != null)
            {
                Debug.Log("[Apply] No selected part. Applying to torso.");
                torso.ApplyDamage(Mathf.RoundToInt(scaledFinalDamage));
            }
            else
            {
                Debug.LogWarning("[Apply] No torso found, applying full damage to character.");
                target.currentHP -= Mathf.RoundToInt(scaledFinalDamage);
            }
        }
        else
        {
            float splitDamage = scaledFinalDamage / targetParts.Count;
            foreach (var part in targetParts)
            {
                part.ApplyDamage(Mathf.RoundToInt(splitDamage));
            }
        }

        return scaledFinalDamage;
    }


    public float CalculateDamageNoCreativity(
        float feasibility, float potential, float baseDamage,
        Character attacker, Character target)
    {
        const float constant = 2f;

        var weaponDamageBreakdown = GetActiveWeaponDamageBreakdown(attacker);
        float totalWeaponDamage = weaponDamageBreakdown.Values.Sum();
        float totalBaseDamage = baseDamage + totalWeaponDamage;

        // 🧠 Apply part-based modifiers (additive)
        float feasibilityModifierSum = target.bodyParts.Sum(p => p.feasibilityModifier);
        float potentialModifierSum = target.bodyParts.Sum(p => p.potentialModifier);

        float finalFeasibility = Mathf.Max(0f, feasibility + feasibilityModifierSum);
        float finalPotential = Mathf.Max(0f, potential + potentialModifierSum);

        // 🛡️ Apply armor reductions (multiplicative)
        foreach (var part in target.bodyParts)
        {
            ArmorData armor = part.equippedArmor;
            if (armor == null) continue;

            finalFeasibility *= 1f - Mathf.Clamp01(armor.reduceFeasibility);
            finalPotential *= 1f - Mathf.Clamp01(armor.reducePotentialDamage);
        }

        float llmDamageModifier = ((finalFeasibility / 10f) * (finalPotential / 10f)) * constant;
        float damageBeforeReduction = totalBaseDamage * llmDamageModifier;

        Debug.Log($"[Enemy Damage Before Reduction]: {damageBeforeReduction}");

        var reducedDamageBreakdown = new Dictionary<DamageType, float>();
        foreach (var kvp in weaponDamageBreakdown)
        {
            DamageType dt = kvp.Key;
            float typeDamage = kvp.Value;
            float reduction = 0f;

            foreach (var part in target.bodyParts)
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

            float reduced = Mathf.Max(0f, typeDamage - reduction);
            reducedDamageBreakdown[dt] = reduced;
        }

        float finalDamage = reducedDamageBreakdown.Values.Sum() + baseDamage;
        float scaledFinalDamage = Mathf.Max(0f, finalDamage * llmDamageModifier);

        Debug.Log($"[Final Damage]: {scaledFinalDamage}");

        List<BodyPartData> targetParts = battleManager.selectedParts;

        if (targetParts == null || targetParts.Count == 0)
        {
            BodyPartData torso = target.bodyParts.FirstOrDefault(p => p.type == BodyPartType.Torso);
            if (torso != null)
            {
                Debug.Log("[Apply] No selected part. Applying to torso.");
                torso.ApplyDamage(Mathf.RoundToInt(scaledFinalDamage));
            }
            else
            {
                Debug.LogWarning("[Apply] No torso found, applying full damage to character.");
                target.currentHP -= Mathf.RoundToInt(scaledFinalDamage);
            }
        }
        else
        {
            float splitDamage = scaledFinalDamage / targetParts.Count;
            foreach (var part in targetParts)
            {
                part.ApplyDamage(Mathf.RoundToInt(splitDamage));
            }
        }

        return scaledFinalDamage;
    }

}

