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
}