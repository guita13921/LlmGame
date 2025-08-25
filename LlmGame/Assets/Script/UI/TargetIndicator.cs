using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    private void LateUpdate()
    {
        if (cam != null)
            transform.rotation = cam.transform.rotation;
    }
}
