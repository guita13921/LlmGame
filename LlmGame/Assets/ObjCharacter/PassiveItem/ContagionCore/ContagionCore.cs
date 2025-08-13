using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ContagionCore : MonoBehaviour, IDeathListener, IPassiveItem
{
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

    public void OnDeath(Character deadCharacter)
    {
        // Check if the dead enemy had Poison or Radiation
        bool hadPoison = deadCharacter.HasStatusEffect(StatusEffectType.Poison);
        bool hadRadiation = deadCharacter.HasStatusEffect(StatusEffectType.Radiation);

        if (!hadPoison && !hadRadiation)
            return;

        BattleManager battleManager = FindObjectOfType<BattleManager>();
        if (battleManager == null)
            return;

        Player player = battleManager.player;
        if (player == null || !player.IsAlive())
            return;

        // Ensure player has this passive equipped
        bool playerHasContagionCore = player.runtimePassiveBehaviors.Any(b => b is ContagionCore);
        if (!playerHasContagionCore)
            return;

        // Apply Poison and Radiation to other living enemies
        foreach (var enemy in battleManager.allCharacters)
        {
            if (enemy == null || enemy == deadCharacter || !enemy.IsAlive()) continue;

            if (hadPoison)
            {
                enemy.ApplyStatusEffect(new TurnStatusEffect(StatusEffectType.Poison, 2, 1));
                Debug.Log($"[ContagionCore] {enemy.characterName} infected with Poison.");
            }

            if (hadRadiation)
            {
                enemy.ApplyStatusEffect(new TurnStatusEffect(StatusEffectType.Radiation, 2, 1));
                Debug.Log($"[ContagionCore] {enemy.characterName} infected with Radiation.");
            }
        }
    }
}
