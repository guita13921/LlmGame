using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target;           // The object to orbit around
    public float distance = 5.0f;      // Distance from the target
    public float orbitSpeed = 100.0f;  // Speed of orbiting

    private float currentX = 0.0f;
    private float currentY = 0.0f;

    public float yMinLimit = -20f;     // Min vertical angle
    public float yMaxLimit = 80f;      // Max vertical angle

    void Update()
    {
        // Horizontal input: A (-1) and D (+1)
        float horizontal = Input.GetAxis("Horizontal");
        // Vertical input: W (+1) and S (-1)
        float vertical = Input.GetAxis("Vertical");

        currentX += horizontal * orbitSpeed * Time.deltaTime;
        currentY -= vertical * orbitSpeed * Time.deltaTime;

        // Clamp vertical angle to avoid flipping
        currentY = Mathf.Clamp(currentY, yMinLimit, yMaxLimit);
    }

    void LateUpdate()
    {
        if (target)
        {
            // Convert angles to rotation
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            // Calculate new position
            Vector3 dir = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = target.position + rotation * dir;

            transform.position = position;
            transform.LookAt(target.position);
        }
    }
}
