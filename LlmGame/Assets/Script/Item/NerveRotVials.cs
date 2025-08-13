using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NerveRotVials : MonoBehaviour, IPassiveItem, IDamageReaction
{
    private const int DEFENSE_REDUCTION = 2;
    private const int MAX_STACKS = 3;
    private const string TAG = "[Nerve Rot Vials]";

    public void ApplyEffect(Character character)
    {
        // Not needed at equip time
    }

    public void DeApplyEffect(Character character) { }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        // Not used
    }

    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        if (target == null || source == null) return;

        // Must have a weapon (or skill) that deals poison or radiation
        var breakdown = GetLastDamageBreakdown(source);
        bool applied = false;

        if (breakdown.TryGetValue(DamageType.Poison, out float poisonDamage) && poisonDamage > 0)
            applied = true;

        if (breakdown.TryGetValue(DamageType.Radiation, out float radDamage) && radDamage > 0)
            applied = true;

        if (!applied) return;

        // Check for custom stacking tracker
        string key = "NerveRot_Stacks";

        int currentStacks = target.TryGetCustomInt(key, out int val) ? val : 0;

        if (currentStacks >= MAX_STACKS)
            return;

        target.defense -= DEFENSE_REDUCTION;
        currentStacks++;
        target.SetCustomInt(key, currentStacks);

        Debug.Log($"{TAG} {source.characterName} applied -{DEFENSE_REDUCTION} DEF to {target.characterName} (Stacks: {currentStacks}/{MAX_STACKS})");
    }

    // Utility method: get last damage breakdown (assumes you track this in CombatHandler or similar)
    private Dictionary<DamageType, float> GetLastDamageBreakdown(Character attacker)
    {
        var combatHandler = FindObjectOfType<CharacterCombatHandler>();
        return combatHandler != null
            ? combatHandler.GetLastDamageBreakdown(attacker)
            : new Dictionary<DamageType, float>();
    }
}
