using UnityEngine;

public class CombatHUDProcessor : MonoBehaviour, IPassiveItem, ITurnListener, IDamageReaction
{
    private Character owner;
    private bool usedThisTurn = false;
    private const int bonusDamage = 20;

    public void ApplyEffect(Character character)
    {
        owner = character;
        Debug.Log("📡 Combat HUD Processor equipped: first ranged attack each turn deals +20 explosive damage.");
    }

    public void DeApplyEffect(Character character)
    {
        if (owner == character)
        {
            owner = null;
            usedThisTurn = false;
        }
    }

    public void OnTurnStart(Character character)
    {
        if (character == owner)
            usedThisTurn = false;
    }

    public void OnTurnEnd(Character character) { }

    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        if (usedThisTurn || source != owner || target == null)
            return;

        Weapon weapon = owner.rightHandWeapon ?? owner.leftHandWeapon;
        if (weapon != null && weapon.weaponType == WeaponType.Ranged_Weapon)
        {
            target.TakeDamage(bonusDamage);

            var handler = FindObjectOfType<CharacterCombatHandler>();
            if (handler != null)
            {
                var breakdown = handler.GetLastDamageBreakdown(owner);
                if (breakdown.ContainsKey(DamageType.Explosive))
                    breakdown[DamageType.Explosive] += bonusDamage;
                else
                    breakdown[DamageType.Explosive] = bonusDamage;
                handler.SaveLastDamageBreakdown(owner, breakdown);
            }

            usedThisTurn = true;
            Debug.Log($"📡 Combat HUD Processor: dealt extra {bonusDamage} explosive damage.");
        }
    }

    public void OnBeforeDamage(Character source, Character target, ref int damage) { }
}
