using UnityEngine;

public class BloodRushCore : MonoBehaviour, IPassiveItem, ITurnListener, IStatusEffectListener, IPossibilityModifier
{
    private Character owner;
    private int bloodThisTurn = 0;
    private bool boostReady;

    private const int bloodThreshold = 1;

    public void ApplyEffect(Character character)
    {
        owner = character;
        ResetState();
        Debug.Log("🩸 Blood Rush Core equipped.");
    }

    public void DeApplyEffect(Character character)
    {
        if (owner == character)
        {
            owner = null;
            ResetState();
        }
    }

    public void OnTurnStart(Character character)
    {
        if (character != owner) return;
        //ResetState();
    }

    private void ResetState()
    {
        bloodThisTurn = 0;
        boostReady = false;
    }

    public void ModifyCritical(Character character, PossibilityPool pool)
    {
        Debug.Log($"[BloodRushCore.ModifyCritical] boostReady: {boostReady}");
        if (boostReady)
        {
            pool.AddCriticalMultiplierBonus(1.0f);
            Debug.Log("💥 BloodRushCore added +1.0x crit multiplier");
        }
    }


    public void OnBleedDamageDealt(Character target, int damage, Character source)
    {
        if (source != owner) return;

        bloodThisTurn += 1;
        boostReady = true;

        Debug.Log($"[BloodRushCore.OnBleedDamageDealt] boostReady: {boostReady}, object: {gameObject.name}, instanceID: {GetInstanceID()}");
    }

    public bool IsReady()
    {
        return boostReady;
    }

    public void Consume()
    {
        boostReady = false;
    }

    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        throw new System.NotImplementedException();
    }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        throw new System.NotImplementedException();
    }

    public void OnTurnEnd(Character character)
    {
        throw new System.NotImplementedException();
    }

    public bool ShouldSpreadBleed(Character character)
    {
        throw new System.NotImplementedException();
    }

    public bool ShouldBlockStatus(Character character, TurnStatusEffect effect)
    {
        throw new System.NotImplementedException();
    }

    public void ModifyChances(PossibilityPool pool)
    {
        throw new System.NotImplementedException();
    }
}
