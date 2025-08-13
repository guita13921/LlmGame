using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NervousSystemSeparator : MonoBehaviour, IPassiveItem
{
    private int focusBoost;

    public void ApplyEffect(Character character)
    {
        BattleManager battleManager = FindAnyObjectByType<BattleManager>();
        focusBoost = battleManager != null ? battleManager.enemies.Count * 2 : 0;
        character.focus += focusBoost;
        character.bonusFocus += focusBoost;
        Debug.Log($"{character.characterName} gains +{focusBoost} Focus from Nervous System Separator.");
    }

    public void DeApplyEffect(Character character)
    {
        character.focus -= focusBoost;
        character.bonusFocus -= focusBoost;
        focusBoost = 0;
    }

    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        throw new System.NotImplementedException();
    }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        throw new System.NotImplementedException();
    }
}
