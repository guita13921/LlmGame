using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Random = UnityEngine.Random;

/// <summary>
/// Chooses a random event prefab and spawns it when the scene loads.
/// Exposes the spawned MysteryEvent via Current and an event.
/// </summary>
public class MysteryEventSystem : MonoBehaviour
{
    public static event Action<MysteryEvent> OnEventSpawned;

    [Tooltip("Prefabs that contain a MysteryEvent somewhere on the root or children.")]
    public List<GameObject> eventPrefabs = new();

    [Header("Backgrounds")]
    public Image backgroundImage; // ← UI background Image component
    public List<EventBackground> eventBackgrounds = new(); // ← Assign in Inspector

    [Serializable]
    public struct EventBackground
    {
        public GameObject eventPrefab;
        public Sprite backgroundSprite;
    }

    /// <summary>The currently spawned event (if any).</summary>
    public MysteryEvent Current { get; private set; }

    private void Start()
    {
        if (eventPrefabs.Count == 0) return;

        int index = Random.Range(0, eventPrefabs.Count);
        GameObject prefab = eventPrefabs[index];
        var instance = Instantiate(prefab, transform);

        // Set background image for this prefab
        SetBackgroundForEvent(prefab);

        // Find the MysteryEvent component on the instance
        Current = instance.GetComponentInChildren<MysteryEvent>(true);
        if (Current == null)
        {
            Debug.LogWarning($"Spawned prefab '{instance.name}' has no MysteryEvent component.");
            return;
        }

        OnEventSpawned?.Invoke(Current);
    }

    private void SetBackgroundForEvent(GameObject prefab)
    {
        if (backgroundImage == null)
        {
            Debug.LogWarning("[MysteryEventSystem] Background image reference not set.");
            return;
        }

        foreach (var bg in eventBackgrounds)
        {
            if (bg.eventPrefab == prefab)
            {
                backgroundImage.sprite = bg.backgroundSprite;
                return;
            }
        }

        Debug.LogWarning($"[MysteryEventSystem] No background sprite set for prefab '{prefab.name}'");
    }
}
