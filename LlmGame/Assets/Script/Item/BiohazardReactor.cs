using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BiohazardReactor : MonoBehaviour, IPassiveItem, IDamageReaction
{
    private const float RESISTANCE = 0.2f; // 20%
    private const float DEBUFF_CHANCE = 0.25f;

    public void ApplyEffect(Character character)
    {
        // No immediate effect
    }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        if (source == null || target == null) return;

        var handler = Object.FindObjectOfType<CharacterCombatHandler>();
        if (handler == null) return;

        Dictionary<DamageType, float> breakdown = handler.GetLastDamageBreakdown(source);
        if (breakdown == null || breakdown.Count == 0) return;

        breakdown.TryGetValue(DamageType.Poison, out float chemical);
        breakdown.TryGetValue(DamageType.Radiation, out float radiation);

        float totalRelevant = chemical + radiation;
        float total = 0f;
        foreach (var v in breakdown.Values) total += v;
        if (totalRelevant <= 0f || total <= 0f) return;

        float ratio = totalRelevant / total;

        int reduceAmount = Mathf.RoundToInt(damage * RESISTANCE * ratio);
        damage = Mathf.Max(0, damage - reduceAmount);
    }

    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        if (source == null || target == null) return;

        Weapon weapon = source.rightHandWeapon ?? source.leftHandWeapon;

        if (weapon == null || weapon.weaponType != WeaponType.Melee_Weapon) return;

        if (Random.value <= DEBUFF_CHANCE)
        {
            var Radiation = new TurnStatusEffect(StatusEffectType.Radiation, 1, 1, target);
            source.ApplyStatusEffect(Radiation);
        }

        if (Random.value <= DEBUFF_CHANCE)
        {
            var Poison = new TurnStatusEffect(StatusEffectType.Poison, 1, 1, target);
            source.ApplyStatusEffect(Poison);
        }
    }
}