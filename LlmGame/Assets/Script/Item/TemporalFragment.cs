using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemporalFragment : MonoBehaviour, IPassiveItem
{
    public void ApplyEffect(Character character)
    {
        // Requires turn manager integration
        var battleManager = FindObjectOfType<BattleManager>();
        if (battleManager != null)
        {
            battleManager.currentActingCharacter = character;
            battleManager.isActionPhase = true;
            Debug.Log($"{character.characterName} will go first thanks to Temporal Fragment.");
        }
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
