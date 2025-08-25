using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class AssignEventCamera : MonoBehaviour
{
    private void Awake()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (canvas.renderMode == RenderMode.WorldSpace)
        {
            if (canvas.worldCamera == null)
            {
                Camera mainCamera = Camera.main;

                // Fallback in case Camera.main is null
                if (mainCamera == null)
                {
                    mainCamera = FindObjectOfType<Camera>();
                }

                if (mainCamera != null)
                {
                    canvas.worldCamera = mainCamera;
                    Debug.Log($"Assigned {mainCamera.name} to {gameObject.name}'s event camera.");
                }
                else
                {
                    Debug.LogWarning("No camera found in the scene to assign to the canvas.");
                }
            }
        }
    }
}
