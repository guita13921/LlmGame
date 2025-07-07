using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBillBoard : MonoBehaviour
{
    [SerializeField] Camera targetCamera;
    [SerializeField] bool freezeXZAxis = true;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    void Update()
    {
        if (targetCamera == null) return;

        if (freezeXZAxis)
        {
            transform.rotation = Quaternion.Euler(0f, targetCamera.transform.rotation.eulerAngles.y, 0f);
        }
        else
        {
            transform.rotation = targetCamera.transform.rotation;
        }
    }
}
