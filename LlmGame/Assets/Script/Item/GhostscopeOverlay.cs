using UnityEngine;

public class GhostscopeOverlay : MonoBehaviour, IPassiveItem
{
    public void ApplyEffect(Character character)
    {
        Debug.Log("👻 Ghostscope Overlay equipped: ranged attacks ignore feasibility and potential penalties from armor.");
    }

    public void DeApplyEffect(Character character) { }

    public void OnAfterDamage(Character source, Character target, int finalDamage) { }
    public void OnBeforeDamage(Character source, Character target, ref int damage) { }
}
