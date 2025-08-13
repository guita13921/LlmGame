using UnityEngine;

public class AdrenalSurge : MonoBehaviour, IPassiveItem, IDamageReaction, ITurnListener
{
    private bool buffActive = false;
    private int boostAmount = 10;

    void Start()
    {
        buffActive = false;
    }

    public void ApplyEffect(Character character)
    {
        // Nothing to do immediately
    }

    public void DeApplyEffect(Character character)
    {
        if (buffActive)
        {
            character.speed -= boostAmount;
            buffActive = false;
        }
    }

    // 🔥 Trigger when character takes damage
    public void OnAfterDamage(Character source, Character target, int finalDamage)
    {
        Debug.Log("AdrenalSurge : OnAfterDamage");
        Debug.Log(buffActive);

        if (!buffActive && target.currentHP <= target.maxHP * 0.5f)
        {
            Debug.Log(target.maxHP * 0.5f);
            Debug.Log(target.currentHP);

            target.speed += boostAmount;
            buffActive = true;
            Debug.Log($"{target.characterName} activated Adrenal Surge: +{boostAmount} Speed after being hit!");
        }
    }

    public void OnBeforeDamage(Character source, Character target, ref int damage)
    {
        // Not needed
    }

    // ⏱ Remove buff at end of turn
    public void OnTurnEnd(Character character)
    {
        if (buffActive)
        {
            character.speed -= boostAmount;
            buffActive = false;
            Debug.Log($"{character.characterName}'s Adrenal Surge wore off.");
        }
    }

    public void OnTurnStart(Character character) { }
}
