using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles displaying all active status effect icons for a character.
/// </summary>
public class CharacterStatusUI : MonoBehaviour
{
    public Character character;
    public RectTransform iconContainer;
    public StatusEffectIconUI iconPrefab;
    public StatusEffectTooltip tooltip;

    private readonly Dictionary<StatusEffectType, StatusEffectIconUI> activeIcons = new();

    private void Awake()
    {
        if (character == null)
            character = GetComponent<Character>();

        if (character != null)
            character.StatusEffectsChanged += Refresh;
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDestroy()
    {
        if (character != null)
            character.StatusEffectsChanged -= Refresh;
    }

    /// <summary>Refreshes the icon list to match the character's active effects.</summary>
    public void Refresh()
    {
        if (character == null || iconContainer == null || iconPrefab == null) return;

        // Remove icons for missing effects
        var toRemove = new List<StatusEffectType>();
        foreach (var kv in activeIcons)
        {
            if (!character.activeStatusEffects.Exists(e => e.effectType == kv.Key))
            {
                Destroy(kv.Value.gameObject);
                toRemove.Add(kv.Key);
            }
        }
        foreach (var type in toRemove) activeIcons.Remove(type);

        // Add or update icons
        foreach (var effect in character.activeStatusEffects)
        {
            if (activeIcons.TryGetValue(effect.effectType, out var icon))
            {
                icon.UpdateData(effect);
            }
            else
            {
                var newIcon = Instantiate(iconPrefab, iconContainer);
                newIcon.Initialize(effect, tooltip);
                activeIcons.Add(effect.effectType, newIcon);
            }
        }
    }
}

