using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedingIncreasingCoating : MonoBehaviour, IPassiveItem, IAttackListener
{
    public void ApplyEffect(Character character) { }

    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        throw new System.NotImplementedException();
    }

    public void OnAttackHit(Character attacker, Character target)
    {
        BattleManager battleManager = FindAnyObjectByType<BattleManager>();
        if (battleManager == null || battleManager.player == null) return;

        // 🔍 Check if any active item is a melee weapon
        bool hasMeleeWeaponEquipped = false;
        foreach (var item in battleManager.player.activeItem)
        {
            if (item is Weapon weapon && weapon.weaponType == WeaponType.Melee_Weapon)
            {
                hasMeleeWeaponEquipped = true;
                break;
            }
        }

        if (!hasMeleeWeaponEquipped)
        {
            Debug.Log("Bleeding Coating not applied: No melee weapon equipped.");
            return;
        }

        // ✅ 50% chance to apply Bleed
        if (Random.value <= 0.5f)
        {
            TurnStatusEffect bleed = new TurnStatusEffect
            (
                StatusEffectType.Bleed,
                target.characterType == CharacterType.Human ? 2 : 1,
                1
            );
            target.ApplyStatusEffect(bleed);

            Debug.Log($"{attacker.characterName} inflicted Bleed on {target.characterName}.");
        }
    }


    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        throw new System.NotImplementedException();
    }
}

