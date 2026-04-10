using System;
using System.Collections;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Top-Down View Settings")]
    public bool allowMovementInTopDown = false; 
    
    public float maxTopDownHoldTime = 3f;
    public float swapBackDelay = 1f;

    public bool IsTopDownActive { get; private set; } = false;

    public Camera MainCam { get; private set; }
    
    private float currentTopDownTimer = 0f;
    private bool canUseTopDown = true;
    private bool isWaitingToSwap = false;

    private CameraFollow cameraFollow;

    void Start()
    {
        MainCam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
    }

    void Update()
    {
        HandleTopDownView();
    }

    private void HandleTopDownView()
    {
        if (isWaitingToSwap) return; 

        // Right Mouse Button
        if (Input.GetMouseButton(1) && canUseTopDown)
        {
            IsTopDownActive = true;
            currentTopDownTimer += Time.deltaTime;

            if (currentTopDownTimer >= maxTopDownHoldTime)
            {
                // Force the player to release the button to use it again
                canUseTopDown = false; 
                StartSwapBack();
            }
        }
        else
        {
            // If we were in top down, but just let go of Right Click
            if (IsTopDownActive) 
            {
                StartSwapBack();
            }
            
            // Reset the ability to use top-down view once the player lets go of Right Click
            if (!Input.GetMouseButton(1))
            {
                currentTopDownTimer = 0f;
                canUseTopDown = true;
            }
        }
        
        // Update camera state if not waiting
        if (!isWaitingToSwap && cameraFollow != null)
        {
            cameraFollow.useTopDownView = IsTopDownActive;
        }
    }

    // Checks if we are holding Left Click before swapping
    private void StartSwapBack()
    {
        if (Input.GetMouseButton(0))
        {
            StartCoroutine(DelaySwapBackRoutine());
        }
        else
        {
            // If not holding Left Click, swap immediately
            IsTopDownActive = false;
            if (cameraFollow != null) cameraFollow.useTopDownView = false;
        }
    }

    private IEnumerator DelaySwapBackRoutine()
    {
        isWaitingToSwap = true;

        // We wait UNTIL the player lets go of Left Mouse,
        yield return new WaitUntil(() => !Input.GetMouseButton(0));

        IsTopDownActive = false;
        if (cameraFollow != null) cameraFollow.useTopDownView = false;
        isWaitingToSwap = false;
    }
}