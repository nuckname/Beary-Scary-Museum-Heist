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

    private Camera mainCam;

    // Locked Y position
    private float lockedY;

    void Start()
    {
        currentStamina = maxStamina;
        mainCam = Camera.main;
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
                isExhausted = true;
            }
        }
        else
        {
            currentSpeed = walkSpeed;

            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
            }

            if (isExhausted && currentStamina >= maxStamina * 0.2f)
            {
                isExhausted = false;
            }
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