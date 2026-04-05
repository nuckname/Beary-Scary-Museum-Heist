using System;
using System.Collections;
using UnityEngine;

// Ensures the new camera script is attached to the same GameObject
[RequireComponent(typeof(PlayerCameraController))] 
public class PlayerStealthController : MonoBehaviour
{
    public float walkSpeed;
    public float sprintSpeed;

    [Header("Stamina Settings")]
    public float maxStamina = 5f;
    public float staminaRegenRate = 1.5f;
    public float staminaDepleteRate = 1f;

    private float currentStamina;
    private bool isExhausted = false;

    public CharacterController controller;
    private Vector3 moveDirection;
    private float currentSpeed;

    // Locked Y position
    private float lockedY;

    private PlayerCameraController cameraController;

    void Start()
    {
        currentStamina = maxStamina;
        cameraController = GetComponent<PlayerCameraController>();
        lockedY = transform.position.y;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        HandleSpeed();
        MovePlayer();
        HandleMouseRotation();
    }

    // Bool function to determine if movement is currently allowed
    private bool CanMoveNormally()
    {
        if (cameraController.IsTopDownActive && !cameraController.allowMovementInTopDown)
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

    void HandleMouseRotation()
    {
        if (cameraController.MainCam == null) return;

        Ray ray = cameraController.MainCam.ScreenPointToRay(Input.mousePosition);
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

    // AI
    // https://gemini.google.com/share/b0fd5f70855e
    void OnGUI()
    {
        Color originalColor = GUI.color;
        float staminaPercent = currentStamina / maxStamina;

        // --- BAR DIMENSIONS (Made Bigger) ---
        float maxBarWidth = 400f; 
        float barThickness = 24f; 

        // --- POSITIONING (Bottom Middle) ---
        float startX = (Screen.width / 2f) - (maxBarWidth / 2f);
        float startY = Screen.height - 50f; // Shifted up slightly to fit the thicker bar

        // --- SPRINT TEXT (Top Right of Bar) ---
        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
        textStyle.alignment = TextAnchor.LowerRight;
        textStyle.normal.textColor = Color.white;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.fontSize = 16; // Increased font size slightly to match the larger bar

        // Position the text right above the right edge of the bar
        Rect textRect = new Rect(startX + maxBarWidth - 100f, startY - 26f, 100f, 20f);
        GUI.Label(textRect, "Sprint", textStyle);

        // --- DRAW BARS ---
        // 1. Draw a semi-transparent black background
        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.DrawTexture(new Rect(startX, startY, maxBarWidth, barThickness), Texture2D.whiteTexture);

        // 2. Generate a shifting rainbow color based on time
        // Mathf.Repeat loops the value between 0 and 1 over time. Multiplier controls speed.
        Color rainbowColor = Color.HSVToRGB(Mathf.Repeat(Time.time * 0.5f, 1f), 1f, 1f);

        // 3. Choose foreground color (Red if exhausted, otherwise Rainbow)
        GUI.color = isExhausted ? Color.red : rainbowColor;

        // 4. Draw the solid stamina block
        float currentWidth = maxBarWidth * staminaPercent;
        Rect activeBarRect = new Rect(startX, startY, currentWidth, barThickness);
        GUI.DrawTexture(activeBarRect, Texture2D.whiteTexture);

        // Restore original color
        GUI.color = originalColor;
    }
}