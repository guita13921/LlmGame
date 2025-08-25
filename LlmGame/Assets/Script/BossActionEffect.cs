using UnityEngine;

public class BossActionEffect : MonoBehaviour
{
    public GameObject effectPrefab;

    public void PlayEffect()
    {
        if (effectPrefab == null) return;
        GameObject fx = Instantiate(effectPrefab, transform.position, Quaternion.identity);
        Destroy(fx, 5f);
    }
}
