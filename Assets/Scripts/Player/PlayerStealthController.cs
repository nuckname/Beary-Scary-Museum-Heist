using System;
using System.Collections;
using UnityEngine;

public class PlayerStealthController : MonoBehaviour
{
    public float walkSpeed;
    public float sprintSpeed;

    [Header("Stamina Settings")]
    public float maxStamina = 5f;
    public float staminaRegenRate = 1.5f;
    public float staminaDepleteRate = 1f;

    [Header("Top-Down View Settings")]
    // Toggle to allow movement while in topdown mode
    public bool allowMovementInTopDown = false; 
    
    public float maxTopDownHoldTime = 3f;
    // How long to wait if holding Left Mouse
    public float swapBackDelay = 1f;
    
    private float currentTopDownTimer = 0f;
    private bool isTopDownActive = false;
    private bool canUseTopDown = true;
    private bool isWaitingToSwap = false;

    private float currentStamina;
    private bool isExhausted = false;

    public CharacterController controller;
    private Vector3 moveDirection;
    private float currentSpeed;

    private Camera mainCam;
    private CameraFollow cameraFollow;

    // Locked Y position
    private float lockedY;

    void Start()
    {
        currentStamina = maxStamina;
        mainCam = Camera.main;
        
        cameraFollow = mainCam.GetComponent<CameraFollow>();
        
        lockedY = transform.position.y;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        HandleTopDownView();
        HandleSpeed();
        MovePlayer();
        HandleMouseRotation();

        // testing
        if (Input.GetKeyDown(KeyCode.P))
        {
            allowMovementInTopDown = !allowMovementInTopDown;
        }
    }

    void HandleTopDownView()
    {
        if (isWaitingToSwap) return; 

        // Right Mouse Button
        if (Input.GetMouseButton(1) && canUseTopDown)
        {
            isTopDownActive = true;
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
            if (isTopDownActive) 
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
        if (!isWaitingToSwap)
        {
            cameraFollow.useTopDownView = isTopDownActive;
        }
    }

    // Checks if we are holding Left Click before swapping
    void StartSwapBack()
    {
        if (Input.GetMouseButton(0))
        {
            StartCoroutine(DelaySwapBackRoutine());
        }
        else
        {
            // If not holding Left Click, swap immediately
            isTopDownActive = false;
            cameraFollow.useTopDownView = false;
        }
    }

    private IEnumerator DelaySwapBackRoutine()
    {
        isWaitingToSwap = true;

        // Wait for a specific amount of time
        //yield return new WaitForSeconds(swapBackDelay);

        // We wait UNTIL the player lets go of Lise Mouse,
        yield return new WaitUntil(() => !Input.GetMouseButton(0));

        isTopDownActive = false;
        cameraFollow.useTopDownView = false;
        isWaitingToSwap = false;
    }

    // Bool function to determine if movement is currently allowed
    private bool CanMoveNormally()
    {
        if (isTopDownActive && !allowMovementInTopDown)
        {
            return false;
        }
        return true;
    }

    void HandleSpeed()
    {
        // If we are looking top-down, stop movement completely
        if (!CanMoveNormally())
        {
            currentSpeed = 0f;
            
            // Allow stamina to regen while resting and looking around
            RegenStamina();
            return;
        }

        bool isMoving = moveDirection.magnitude >= 0.1f;
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift) && isMoving && !isExhausted;

        if (isTryingToSprint)
        {
            currentSpeed = sprintSpeed;
            currentStamina -= staminaDepleteRate * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true;
            }
        }
        else
        {
            currentSpeed = walkSpeed;

            RegenStamina();
        }
    }

    private void RegenStamina()
    {
        if (currentStamina < maxStamina)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
        }

        if (isExhausted && currentStamina >= maxStamina * 0.2f)
        {
            isExhausted = false;
        }
    }

    void MovePlayer()
    {
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);

        // Lock Y position
        Vector3 pos = transform.position;
        pos.y = lockedY;
        transform.position = pos;
    }

    // AI https://gemini.google.com/share/3ac24891bcfb
    void HandleMouseRotation()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Vector3 lookDirection = point - transform.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
            }
        }
    }

void OnGUI()
    {
        Color originalColor = GUI.color;
        float staminaPercent = currentStamina / maxStamina;
        // ==========================================
        // 2. NEW UI: Horizontal Stamina Bar Above Player
        // ==========================================
        
        // Pick a point above the player's head (2.0f units up). 
        // Adjust this number if it floats too high or low!
        Vector3 headPosition = transform.position + (Vector3.up * 2.0f);
        
        // Convert that 3D head position to 2D screen coordinates
        Vector3 screenPos = mainCam.WorldToScreenPoint(headPosition);

        // Only draw if the player is in front of the camera
        if (screenPos.z > 0)
        {
            // Unity Screen coordinates start at the bottom-left. GUI starts at top-left.
            float guiY = Screen.height - screenPos.y;

            // --- BAR DIMENSIONS ---
            float maxBarWidth = 60f; // Total width when stamina is full
            float barThickness = 8f; // How thick the line is top-to-bottom

            // Calculate the current width based on stamina percentage
            float currentWidth = maxBarWidth * staminaPercent;

            // Calculate X position so the bar is perfectly centered over the player
            float startX = screenPos.x - (maxBarWidth / 2f);

            // Create the rectangle: (X, Y, Width, Height)
            Rect lineRect = new Rect(startX, guiY, currentWidth, barThickness);

            // Choose color based on exhaustion state
            GUI.color = isExhausted ? Color.red : Color.cyan;

            // Draw the solid block
            GUI.DrawTexture(lineRect, Texture2D.whiteTexture);
        }

        // Restore original color
        GUI.color = originalColor;
    }
}