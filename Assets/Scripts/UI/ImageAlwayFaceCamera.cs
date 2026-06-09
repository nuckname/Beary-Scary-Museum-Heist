using System;
using UnityEngine;

public class ImageAlwayFaceCamera : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
    }

    void LateUpdate()
    {
        // Makes the object's forward direction perfectly match the camera's forward direction.
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, 
            mainCamera.transform.rotation * Vector3.up);
    }
}