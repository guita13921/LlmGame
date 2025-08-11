using System.Collections.Generic;
using UnityEngine;
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

    /// <summary>The currently spawned event (if any).</summary>
    public MysteryEvent Current { get; private set; }

    private void Start()
    {
        if (eventPrefabs.Count == 0) return;

        int index = Random.Range(0, eventPrefabs.Count);
        var instance = Instantiate(eventPrefabs[index], transform);

        // Find the MysteryEvent component on the instance
        Current = instance.GetComponentInChildren<MysteryEvent>(true);
        if (Current == null)
        {
            Debug.LogWarning($"Spawned prefab '{instance.name}' has no MysteryEvent component.");
            return;
        }

        OnEventSpawned?.Invoke(Current);
    }
}
