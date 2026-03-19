using UnityEngine;

public class PlayerStealthController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    
    [Header("Stamina Settings")]
    public float maxStamina = 5f;
    public float staminaRegenRate = 1.5f;
    public float staminaDepleteRate = 1f;
    
    private float currentStamina;
    private bool isExhausted = false;

    public CharacterController controller;
    private Vector3 moveDirection;
    private float currentSpeed;

    void Start()
    {
        currentStamina = maxStamina;
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        HandleSpeed();
        MovePlayer();
    }

    void HandleSpeed()
    {
        bool isMoving = moveDirection.magnitude >= 0.1f;
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift) && isMoving && !isExhausted;

        if (isTryingToSprint)
        {
            currentSpeed = sprintSpeed;
            currentStamina -= staminaDepleteRate * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true; // Force player to stop sprinting
            }
        }
        else
        {
            currentSpeed = walkSpeed;
            
            // Regenerate stamina when not sprinting
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
            }

            // Reset exhausted state once stamina has recovered a bit (20% right now)
            if (isExhausted && currentStamina >= maxStamina * 0.2f)
            {
                isExhausted = false;
            }
        }
    }

    void MovePlayer()
    {
        controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        
        if (moveDirection.magnitude >= 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
        }
    }

    //AI
    void OnGUI()
    {
        // 1. Define the size and position of the background bar
        float barWidth = 25f;
        float barHeight = 200f;
        float xPos = 20f; // 20 pixels from the left of the screen
        float yPos = Screen.height - barHeight - 20f; // 20 pixels from the bottom

        // 2. Draw the background (empty) bar in dark gray
        Color originalColor = GUI.color;
        GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        GUI.Box(new Rect(xPos, yPos, barWidth, barHeight), "");

        // 3. Calculate stamina percentage (0.0 to 1.0)
        float staminaPercent = currentStamina / maxStamina;

        // 4. Calculate the height of the filled portion
        float fillHeight = barHeight * staminaPercent;
        
        // 5. Offset the Y position so the bar fills from bottom to top
        float fillYPos = yPos + (barHeight - fillHeight);

        // 6. Change color based on exhaustion state
        if (isExhausted)
        {
            GUI.color = Color.red;
        }
        else
        {
            GUI.color = Color.green;
        }

        // 7. Draw the filled stamina bar over the background
        GUI.Box(new Rect(xPos, fillYPos, barWidth, fillHeight), "");

        // Reset the GUI color back to normal
        GUI.color = originalColor;
    }
}