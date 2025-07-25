using UnityEngine;

[System.Serializable]
public class HitEffectData
{
    [Tooltip("Portion of total damage for this hit, e.g., 0.3 = 30% of final damage.")]
    public float damagePortion = 1.0f;

    [Tooltip("Prefab to spawn as VFX for this hit.")]
    public GameObject vfxPrefab;

    [Tooltip("Sound effect to play for this hit.")]
    public AudioClip sfxClip;

    [Tooltip("Offset relative to target for spawning VFX.")]
    public Vector3 vfxOffset = Vector3.zero;

    [Tooltip("How long the VFX lives.")]
    public float lifeTime = 2f;

}
