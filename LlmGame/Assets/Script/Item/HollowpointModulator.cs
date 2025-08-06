using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HollowpointModulator : PassiveItemBase, IDamageReaction
{
    public override void ApplyEffect(Character character)
    {
        Debug.Log("Hollowpoint Modulator equipped!");
        // Could add visuals/sfx here if desired
    }

    public new void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        if (source == null || !(source is Character attacker)) return;

        if (attacker.isCritical)
        {
            // Apply raw attack damage without defense
            int originalDamage = damage;
            damage += target.defense; // nullify defense manually
            Debug.Log($"[HollowpointModulator] Critical hit! Ignoring {target.defense} defense. {originalDamage} → {damage}");
        }
    }

}
