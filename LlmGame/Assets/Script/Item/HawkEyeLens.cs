using UnityEngine;

public class HawkEyeLens : MonoBehaviour, IPassiveItem
{
    [SerializeField] private float critBonus = 0.5f; // +50% Critical Chance

    public void ApplyEffect(Character character)
    {
        if (character is Player player)
        {
            Weapon weapon = player.rightHandWeapon ?? player.leftHandWeapon;
            if (weapon != null && weapon.weaponType == WeaponType.Ranged_Weapon)
            {
                player.possibilityPool.AddModifier(StatusChanceType.Critical, critBonus);
                Debug.Log($"🎯 Hawk-Eye Lens equipped: +{critBonus * 100}% Critical Chance with ranged weapons.");
            }
        }
    }

    public void OnAfterDamage(Character source, Character target, int finalDamage) { }
    public void OnBeforeDamage(Character source, Character target, ref int damage) { }
}
