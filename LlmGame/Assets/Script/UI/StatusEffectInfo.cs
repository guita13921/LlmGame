using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Helper database providing icon and description data for each <see cref="StatusEffectType"/>.
/// Icons are loaded from Resources/StatusEffects/{EffectType}.png by default.
/// </summary>
public static class StatusEffectInfo
{
    private static readonly Dictionary<StatusEffectType, string> descriptions = new()
    {
        { StatusEffectType.Stun, "Unable to act during their next turn." },
        { StatusEffectType.DefenseDown, "Defense is reduced." },
        { StatusEffectType.AttackDown, "Attack power is reduced." },
        { StatusEffectType.FocusDown, "Focus is reduced." },
        { StatusEffectType.Bleed, "Loses HP each turn." },
        { StatusEffectType.Poison, "Takes poison damage over time." },
        { StatusEffectType.Radiation, "Radiated - may trigger additional effects." },
        { StatusEffectType.Contaminated, "Suffers combined poison and radiation effects." },
        { StatusEffectType.AttackUp, "Attack power is increased." },
        { StatusEffectType.DefenseUp, "Defense is increased." },
        { StatusEffectType.SpeedUp, "Speed is increased." },
        { StatusEffectType.CritChanceUp, "Critical hit chance is increased." },
        { StatusEffectType.CritDamageUp, "Critical hit damage is increased." },
        { StatusEffectType.HealReduction, "Healing received is reduced." }
    };

    /// <summary>Returns a human readable description for the given effect.</summary>
    public static string GetDescription(StatusEffectType type)
        => descriptions.TryGetValue(type, out var desc) ? desc : string.Empty;

    /// <summary>
    /// Returns the sprite for the effect. Looks under Resources/StatusEffects/ using the enum name.
    /// </summary>
    public static Sprite GetIcon(StatusEffectType type)
        => Resources.Load<Sprite>($"StatusEffects/{type}");
}

