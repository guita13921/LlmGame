using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EmbeddedPainkiller : MonoBehaviour, IPassiveItem, IDamageReaction
{
    public float damageReductionPercent = 5f;

    public void ApplyEffect(Character character)
    {
        Debug.Log("ApplyEffect EmbeddedPainkiller");
        character.maxHP += 15;
        character.currentHP += 15;
    }

    public void DeApplyEffect(Character character)
    {
        character.maxHP -= 15;
        character.currentHP = Mathf.Clamp(character.currentHP - 15, 0, character.maxHP);
    }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        float reduced = damage * (damageReductionPercent / 100f);
        damage -= Mathf.RoundToInt(reduced);
        Debug.Log($"Embedded Painkiller reduced damage by {Mathf.RoundToInt(reduced)}");
    }

    public void OnAfterDamage(Character source, Character target, int finalDamage) { }
}
