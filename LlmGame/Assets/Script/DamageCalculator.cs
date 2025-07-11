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

    public float GetActiveWeaponDamage(Character character)
    {
        float totalWeaponDamage = 0f;

        foreach (var activeItem in character.activeItem)
        {
            if (activeItem is Weapon weapon)
            {
                totalWeaponDamage += weapon.Damage;
                Debug.Log($"Adding weapon damage: {weapon.itemName} = {weapon.Damage}");
            }
        }

        Debug.Log($"{character.characterName} total active weapon damage: {totalWeaponDamage}");
        return totalWeaponDamage;
    }

    public float CalculateDamage(float feasibility, float potential, float baseDamage, string userMessage, Character actor)
    {
        const float constant = 2f;

        float activeWeaponDamage = GetActiveWeaponDamage(actor);
        float totalBaseDamage = baseDamage + activeWeaponDamage;

        float llmDamageModifier = ((feasibility / 10) * (potential / 10)) * constant;
        float llmScaledBaseDamage = totalBaseDamage * llmDamageModifier;

        float creativityBonus = CalculateCreativityBonus(userMessage, actor);
        float totalDamage = llmScaledBaseDamage * (1 + creativityBonus);

        Debug.Log($"Damage Calculation: Feasibility={feasibility}, Potential={potential}, BaseDamage={baseDamage}");
        Debug.Log($"ActiveWeaponDamage={activeWeaponDamage}, TotalBaseDamage={totalBaseDamage}");
        Debug.Log($"LLMDamageModifier={llmDamageModifier}, LLMScaledBaseDamage={llmScaledBaseDamage}");
        Debug.Log($"CreativityBonus={creativityBonus}, TotalDamage={totalDamage}");

        return totalDamage;
    }

    public float CalculateDamageNoCreativity(float feasibility, float potential, float baseDamage, Character attacker)
    {
        const float constant = 2f;

        float activeWeaponDamage = GetActiveWeaponDamage(attacker);
        float totalBaseDamage = baseDamage + activeWeaponDamage;

        float llmDamageModifier = ((feasibility / 10) * (potential / 10)) * constant;
        float llmScaledBaseDamage = totalBaseDamage * llmDamageModifier;

        Debug.Log($"Enemy Damage Calculation: Feasibility={feasibility}, Potential={potential}, BaseDamage={baseDamage}");
        Debug.Log($"ActiveWeaponDamage={activeWeaponDamage}, TotalBaseDamage={totalBaseDamage}");
        Debug.Log($"LLMDamageModifier={llmDamageModifier}, LLMScaledBaseDamage={llmScaledBaseDamage}");

        return llmScaledBaseDamage;
    }
}
