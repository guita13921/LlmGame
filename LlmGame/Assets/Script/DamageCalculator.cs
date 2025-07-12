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
        {
            Debug.LogError("DamageCalculator requires BattleManager on the same GameObject.");
        }
    }

    public float CalculateCreativityBonus(string userMessage, Character actor)
    {
        if (string.IsNullOrEmpty(userMessage))
            return 0f;

        List<string> pastMessages = battleManager.GetPastMessagesFromActor(actor);
        pastMessages.Add(userMessage);

        string combinedText = string.Join(" ", pastMessages).ToLower();
        string[] words = combinedText
            .Split(new char[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}' },
                   System.StringSplitOptions.RemoveEmptyEntries);

        HashSet<string> uniqueWords = new HashSet<string>(words);
        int nUniqueWords = uniqueWords.Count;

        Dictionary<string, int> wordCount = new Dictionary<string, int>();
        foreach (string word in words)
        {
            if (wordCount.ContainsKey(word))
                wordCount[word]++;
            else
                wordCount[word] = 1;
        }

        int nWordsUsedAtLeast2Times = wordCount.Count(kvp => kvp.Value >= 2);

        float uniqueWordBonus = Mathf.Min(1f, nUniqueWords * 0.05f);
        float repetitionPenalty = Mathf.Min(0.3f, nWordsUsedAtLeast2Times * 0.02f);

        float creativityBonus = uniqueWordBonus - repetitionPenalty;

        Debug.Log($"Creativity Calculation: UniqueWords={nUniqueWords}, RepeatedWords={nWordsUsedAtLeast2Times}");
        Debug.Log($"UniqueWordBonus={uniqueWordBonus}, RepetitionPenalty={repetitionPenalty}, CreativityBonus={creativityBonus}");

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
                    int value = 0;

                    switch (dt)
                    {
                        case DamageType.Physical: value = weapon.damagePhysical; break;
                        case DamageType.Fire: value = weapon.damageFire; break;
                        case DamageType.Electric: value = weapon.damageElectric; break;
                        case DamageType.Radiation: value = weapon.damageRadiation; break;
                        case DamageType.Explosive: value = weapon.damageExplosive; break;
                        case DamageType.Digital: value = weapon.damageDigital; break;
                        case DamageType.Plasma: value = weapon.damagePlasma; break;
                        case DamageType.Laser: value = weapon.damageLaser; break;
                        case DamageType.Chemical: value = weapon.damageChemical; break;
                        case DamageType.Viral: value = weapon.damageViral; break;
                        default: break;
                    }

                    if (!damageBreakdown.ContainsKey(dt))
                        damageBreakdown[dt] = value;
                    else
                        damageBreakdown[dt] += value;
                }
            }
        }

        return damageBreakdown;
    }

    public float CalculateDamage(float feasibility, float potential, float baseDamage, string userMessage, Character attacker, Character target)
    {
        const float constant = 2f;

        var weaponDamageBreakdown = GetActiveWeaponDamageBreakdown(attacker);

        float totalWeaponDamage = weaponDamageBreakdown.Values.Sum();
        float totalBaseDamage = baseDamage + totalWeaponDamage;

        float llmDamageModifier = ((feasibility / 10f) * (potential / 10f)) * constant;
        float llmScaledBaseDamage = totalBaseDamage * llmDamageModifier;

        float creativityBonus = CalculateCreativityBonus(userMessage, attacker);
        float damageBeforeReduction = llmScaledBaseDamage * (1 + creativityBonus);

        Debug.Log($"Damage Calculation (Before Reduction): Feasibility={feasibility}, Potential={potential}, BaseDamage={baseDamage}, WeaponDamage={totalWeaponDamage}, TotalBaseDamage={totalBaseDamage}");
        Debug.Log($"LLMDamageModifier={llmDamageModifier}, LLMScaledBaseDamage={llmScaledBaseDamage}, CreativityBonus={creativityBonus}, DamageBeforeReduction={damageBeforeReduction}");

        // Apply type-specific reductions
        var reducedDamageBreakdown = new Dictionary<DamageType, float>();

        foreach (var kvp in weaponDamageBreakdown)
        {
            DamageType dt = kvp.Key;
            float typeDamage = kvp.Value;

            float reductionValue = 0f;

            foreach (var defItem in target.activeItem.OfType<Defensive>())
            {
                switch (dt)
                {
                    case DamageType.Physical: reductionValue += defItem.reduceDamagePhysical; break;
                    case DamageType.Fire: reductionValue += defItem.reduceDamageFire; break;
                    case DamageType.Electric: reductionValue += defItem.reduceDamageElectric; break;
                    case DamageType.Radiation: reductionValue += defItem.reduceDamageRadiation; break;
                    case DamageType.Explosive: reductionValue += defItem.reduceDamageExplosive; break;
                    case DamageType.Digital: reductionValue += defItem.reduceDamageDigital; break;
                    case DamageType.Plasma: reductionValue += defItem.reduceDamagePlasma; break;
                    case DamageType.Laser: reductionValue += defItem.reduceDamageLaser; break;
                    case DamageType.Chemical: reductionValue += defItem.reduceDamageChemical; break;
                    case DamageType.Viral: reductionValue += defItem.reduceDamageViral; break;
                    default: break;
                }
            }

            float reducedTypeDamage = Mathf.Max(0f, typeDamage - reductionValue);
            reducedDamageBreakdown[dt] = reducedTypeDamage;

            Debug.Log($"Total reduction for {dt}: {reductionValue}, Damage after reduction: {reducedTypeDamage}");
        }

        float finalDamage = reducedDamageBreakdown.Values.Sum() + baseDamage;
        float scaledFinalDamage = Mathf.Max(0f, finalDamage * llmDamageModifier * (1 + creativityBonus));

        Debug.Log($"Final Damage (After Reductions and Modifiers): {scaledFinalDamage}");

        return scaledFinalDamage;
    }

    public float CalculateDamageNoCreativity(float feasibility, float potential, float baseDamage, Character attacker, Character target)
    {
        const float constant = 2f;

        var weaponDamageBreakdown = GetActiveWeaponDamageBreakdown(attacker);

        float totalWeaponDamage = weaponDamageBreakdown.Values.Sum();
        float totalBaseDamage = baseDamage + totalWeaponDamage;

        float llmDamageModifier = ((feasibility / 10f) * (potential / 10f)) * constant;
        float damageBeforeReduction = totalBaseDamage * llmDamageModifier;

        Debug.Log($"Enemy Damage Calculation (Before Reduction): Feasibility={feasibility}, Potential={potential}, BaseDamage={baseDamage}, WeaponDamage={totalWeaponDamage}, TotalBaseDamage={totalBaseDamage}");
        Debug.Log($"LLMDamageModifier={llmDamageModifier}, DamageBeforeReduction={damageBeforeReduction}");

        // Apply type-specific reductions
        var reducedDamageBreakdown = new Dictionary<DamageType, float>();

        foreach (var kvp in weaponDamageBreakdown)
        {
            DamageType dt = kvp.Key;
            float typeDamage = kvp.Value;

            float reductionValue = 0f;

            foreach (var defItem in target.activeItem.OfType<Defensive>())
            {
                switch (dt)
                {
                    case DamageType.Physical: reductionValue += defItem.reduceDamagePhysical; break;
                    case DamageType.Fire: reductionValue += defItem.reduceDamageFire; break;
                    case DamageType.Electric: reductionValue += defItem.reduceDamageElectric; break;
                    case DamageType.Radiation: reductionValue += defItem.reduceDamageRadiation; break;
                    case DamageType.Explosive: reductionValue += defItem.reduceDamageExplosive; break;
                    case DamageType.Digital: reductionValue += defItem.reduceDamageDigital; break;
                    case DamageType.Plasma: reductionValue += defItem.reduceDamagePlasma; break;
                    case DamageType.Laser: reductionValue += defItem.reduceDamageLaser; break;
                    case DamageType.Chemical: reductionValue += defItem.reduceDamageChemical; break;
                    case DamageType.Viral: reductionValue += defItem.reduceDamageViral; break;
                    default: break;
                }
            }

            float reducedTypeDamage = Mathf.Max(0f, typeDamage - reductionValue);
            reducedDamageBreakdown[dt] = reducedTypeDamage;

            Debug.Log($"Total reduction for {dt}: {reductionValue}, Damage after reduction: {reducedTypeDamage}");
        }

        float finalDamage = reducedDamageBreakdown.Values.Sum() + baseDamage;
        float scaledFinalDamage = Mathf.Max(0f, finalDamage * llmDamageModifier);

        Debug.Log($"Final Damage (After Reductions and Modifiers): {scaledFinalDamage}");

        return scaledFinalDamage;
    }
}
