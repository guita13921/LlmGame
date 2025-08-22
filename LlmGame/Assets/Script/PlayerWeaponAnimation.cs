using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles switching idle and attack animations based on the player's equipped weapon.
/// Attack animations are chosen randomly from a set defined per weapon.
/// </summary>
[RequireComponent(typeof(Player))]
public class PlayerWeaponAnimation : MonoBehaviour
{
    [System.Serializable]
    public class WeaponAnimationSet
    {
        public string weaponName;               // Name from Weapon.itemName
        public AnimationClip idleAnimation;     // Idle animation for this weapon
        public AnimationClip[] attackAnimations; // Attack animations for this weapon
    }

    [SerializeField] private Animator animator;
    [SerializeField] private AnimationClip baseIdleClip;    // Clip used for Idle in the base controller
    [SerializeField] private AnimationClip baseAttackClip;  // Clip used for Attack in the base controller
    [SerializeField] private List<WeaponAnimationSet> weaponAnimationSets = new();

    private Dictionary<string, WeaponAnimationSet> lookup = new();
    private AnimatorOverrideController overrideController;
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
        if (animator == null)
            animator = GetComponent<Animator>();

        // Prepare override controller
        overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = overrideController;

        foreach (var set in weaponAnimationSets)
        {
            if (!string.IsNullOrEmpty(set.weaponName) && !lookup.ContainsKey(set.weaponName))
            {
                lookup.Add(set.weaponName, set);
            }
        }
    }

    private void Start()
    {
        OnWeaponEquipped(player != null ? player.equippedWeapon : null);
    }

    /// <summary>
    /// Call when the player's weapon changes to update idle animation.
    /// </summary>
    public void OnWeaponEquipped(Weapon weapon)
    {
        if (weapon == null)
        {
            if (baseIdleClip != null)
                overrideController[baseIdleClip] = baseIdleClip; // revert to default
            return;
        }

        if (lookup.TryGetValue(weapon.itemName, out var set))
        {
            if (set.idleAnimation != null && baseIdleClip != null)
            {
                overrideController[baseIdleClip] = set.idleAnimation;
            }
        }
    }

    /// <summary>
    /// Randomly selects an attack animation for the current weapon and triggers the Attack parameter.
    /// </summary>
    public void PlayAttack()
    {
        Weapon weapon = player != null ? player.equippedWeapon : null;
        if (weapon != null && lookup.TryGetValue(weapon.itemName, out var set))
        {
            if (set.attackAnimations != null && set.attackAnimations.Length > 0 && baseAttackClip != null)
            {
                var clip = set.attackAnimations[Random.Range(0, set.attackAnimations.Length)];
                overrideController[baseAttackClip] = clip;
            }
        }

        animator.SetTrigger("Attack");
    }
}
