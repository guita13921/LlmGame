using UnityEngine;

public class InjectorGauntlets : MonoBehaviour, IPassiveItem, IDamageReaction
{
    private const float STATUS_CHANCE = 0.5f;

    public void ApplyEffect(Character character)
    {
        // No immediate effect
    }

    public void DeApplyEffect(Character character) { }

    public void OnBeforeDamage(Character source, Character target, ref int damage) { }

    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        if (source == null || target == null) return;

        Weapon weapon = source.equippedWeapon;
        if (weapon == null || weapon.weaponType != WeaponType.Melee_Weapon) return;

        if (Random.value <= STATUS_CHANCE)
        {
            var poison = new TurnStatusEffect(StatusEffectType.Poison, 3, 1, source);
            target.ApplyStatusEffect(poison);
        }

        if (Random.value <= STATUS_CHANCE)
        {
            var radiation = new TurnStatusEffect(StatusEffectType.Radiation, 2, 1, source);
            target.ApplyStatusEffect(radiation);
        }
    }
}