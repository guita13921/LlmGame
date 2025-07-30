using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NervousSystemSeparator : MonoBehaviour, IPassiveItem
{
    public void ApplyEffect(Character character)
    {
        BattleManager battleManager = FindAnyObjectByType<BattleManager>();

        int focusBoost = battleManager.enemies.Count * 2;
        character.focus += focusBoost;

        Debug.Log($"{character.characterName} gains +{focusBoost} Focus from Nervous System Separator.");
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
