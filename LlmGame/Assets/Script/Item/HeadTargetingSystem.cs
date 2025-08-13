using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadTargetingSystem : MonoBehaviour, IPassiveItem, IHitModifier
{
    [SerializeField] private float headDamageBonus = 0.25f;

    public void ApplyEffect(Character character) { }

    public void DeApplyEffect(Character character) { }

    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        throw new System.NotImplementedException();
    }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        throw new System.NotImplementedException();
    }

    public void OnHit(Character attacker, BodyPartData targetPart, ref int damage)
    {
        if (targetPart.type == BodyPartType.Head)
        {
            int bonusDamage = Mathf.RoundToInt(damage * headDamageBonus);
            damage += bonusDamage;
            Debug.Log($"[HeadTargetingSystem] +{bonusDamage} bonus damage to {targetPart.type} (Total: {damage})");
        }
    }
}
