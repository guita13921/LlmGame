using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Chooses a random event prefab and spawns it when the scene loads.
/// </summary>
public class MysteryEventSystem : MonoBehaviour
{
    public List<GameObject> eventPrefabs = new();

    private void Start()
    {
        if (eventPrefabs.Count == 0) return;
        int index = Random.Range(0, eventPrefabs.Count);
        Instantiate(eventPrefabs[index], transform);
    }
}
