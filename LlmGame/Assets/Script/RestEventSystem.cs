using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// Spawns a random RestEvent prefab on scene load and sets a background image.
/// Prioritizes RestEvent.backgroundOverride; else uses type → sprite mapping.
/// </summary>
public class RestEventSystem : MonoBehaviour
{
    public static event Action<RestEvent> OnEventSpawned;

    [Tooltip("Prefabs that contain a RestEvent on the root or children.")]
    public List<GameObject> eventPrefabs = new();

    [Header("Backgrounds (Optional)")]
    public Image backgroundImage;

    [Tooltip("Type → Background mapping if the event doesn't define an override.")]
    public List<RestBackground> restBackgrounds = new();

    [Serializable]
    public struct RestBackground
    {
        public RestEventType type;
        public Sprite backgroundSprite;
    }

    /// <summary>The currently spawned rest event (if any).</summary>
    public RestEvent Current { get; private set; }

    private void Start()
    {
        if (eventPrefabs == null || eventPrefabs.Count == 0) return;

        int index = Random.Range(0, eventPrefabs.Count);
        GameObject prefab = eventPrefabs[index];
        var instance = Instantiate(prefab, transform);

        // Find the RestEvent component on the instance
        Current = instance.GetComponentInChildren<RestEvent>(true);
        if (Current == null)
        {
            Debug.LogWarning($"[RestEventSystem] Spawned prefab '{instance.name}' has no RestEvent component.");
            return;
        }

        SetBackgroundFor(Current);
        OnEventSpawned?.Invoke(Current);
    }

    private void SetBackgroundFor(RestEvent restEvent)
    {
        if (backgroundImage == null) return;

        // 1) Use per-event override if provided
        if (restEvent.backgroundOverride != null)
        {
            backgroundImage.sprite = restEvent.backgroundOverride;
            return;
        }

        // 2) Fallback to type mapping
        foreach (var bg in restBackgrounds)
        {
            if (bg.type == restEvent.eventType)
            {
                backgroundImage.sprite = bg.backgroundSprite;
                return;
            }
        }

        Debug.LogWarning($"[RestEventSystem] No background sprite set for type '{restEvent.eventType}'.");
    }
}
