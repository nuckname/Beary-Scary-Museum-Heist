using System;
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
    private float currentTopDownTimer = 0f;
    private bool isTopDownActive = false;
    private bool canUseTopDown = true;

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
        // Right Mouse Button
        if (Input.GetMouseButton(1) && canUseTopDown)
        {
            isTopDownActive = true;
            currentTopDownTimer += Time.deltaTime;

            if (currentTopDownTimer >= maxTopDownHoldTime)
            {
                isTopDownActive = false;
                
                // Force the player to release the button to use it again
                canUseTopDown = false; 
            }
        }
        else
        {
            isTopDownActive = false;
            
            // Reset the ability to use top-down view once the player lets go of Right Click
            if (!Input.GetMouseButton(1))
            {
                currentTopDownTimer = 0f;
                canUseTopDown = true;
            }
        }
        
        cameraFollow.useTopDownView = isTopDownActive;
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
        float barWidth = 25f;
        float barHeight = 200f;
        float xPos = 20f;
        float yPos = Screen.height - barHeight - 20f;

        Color originalColor = GUI.color;

        GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        GUI.Box(new Rect(xPos, yPos, barWidth, barHeight), "");

        float staminaPercent = currentStamina / maxStamina;
        float fillHeight = barHeight * staminaPercent;
        float fillYPos = yPos + (barHeight - fillHeight);

        if (isExhausted)
        {
            GUI.color = Color.red;
        }
        else
        {
            GUI.color = Color.green;
        }

        GUI.Box(new Rect(xPos, fillYPos, barWidth, fillHeight), "");

        GUI.color = originalColor;
    }
}