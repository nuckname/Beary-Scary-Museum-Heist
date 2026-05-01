using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(PlayerCameraController))] 
public class PlayerStealthController : MonoBehaviour
{
    public float sneakSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    [Header("Stamina Settings")]
    public float maxStamina = 5f;
    public float staminaRegenRate = 1.5f;
    public float staminaDepleteRate = 1f;

    [Header("Footstep Settings")]
    public float walkStepInterval = 0.5f;
    public float sprintStepInterval = 0.3f;
    public float sneakStepInterval = 0.8f;
    private float stepTimer;

    [Header("UI Settings")]
    public Transform staminaBarLocation;

    private float currentStamina;
    private bool isExhausted = false;

    public bool IsSneaking { get; private set; } 

    public Rigidbody rb;
    private Vector3 moveDirection;
    private float currentSpeed;

    [SerializeField] private Animator animator;
    
    private float lockedY;
    private Camera playerCamera;

    private void Awake()
    {
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
    }

    void Start()
    {
        currentStamina = maxStamina;
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
        
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        bool isMoving = input.sqrMagnitude > 0.01f; 
        animator.SetBool("IsMoving", isMoving);
        
        // ??
        // animator.SetBool("IsSneaking", IsSneaking); 

        HandleFootsteps(isMoving);
    }

    void HandleFootsteps(bool isMoving)
    {
        if (isMoving)
        {
            stepTimer -= Time.deltaTime;
            
            if (stepTimer <= 0f)
            {
                if (AudioManager.instance != null)
                {
                    AudioManager.instance.PlayFootstep();
                }

                if (currentSpeed == sprintSpeed)
                    stepTimer = sprintStepInterval;
                else if (currentSpeed == sneakSpeed)
                    stepTimer = sneakStepInterval;
                else
                    stepTimer = walkStepInterval;
            }
        }
        else
        {
            stepTimer = 0f; 
        }
    }

    void HandleSpeed()
    {
        bool isMoving = moveDirection.magnitude >= 0.1f;
        bool isTryingToSprint = Input.GetKey(KeyCode.LeftShift) && isMoving && !isExhausted;
        bool isTryingToSneak = Input.GetKey(KeyCode.LeftControl) && isMoving;

        if (isTryingToSprint)
        {
            currentSpeed = sprintSpeed;
            IsSneaking = false; // Cannot sneak while sprinting
            currentStamina -= staminaDepleteRate * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true;
            }
        }
        else if (isTryingToSneak)
        {
            currentSpeed = sneakSpeed;
            IsSneaking = true;
            RegenStamina();
        }
        else
        {
            currentSpeed = walkSpeed;
            IsSneaking = false;
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
        rb.linearVelocity = moveDirection * currentSpeed;

        Vector3 pos = transform.position;
        pos.y = lockedY;
        transform.position = pos;
    }

    void HandleMouseRotation()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float rayDistance))
        {
            Vector3 point = ray.GetPoint(rayDistance);
            Vector3 lookDirection = point - transform.position;
            lookDirection.y = 0f;

            if (lookDirection.sqrMagnitude > 0.5f) 
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 720f * Time.deltaTime);
            }
        }
    }

    void OnGUI()
    {
        Color originalColor = GUI.color;
        float staminaPercent = currentStamina / maxStamina;
        float maxBarWidth = 400f; 
        float barThickness = 24f; 
        
        // Override theses
        float startX = (Screen.width / 2f) - (maxBarWidth / 2f);
        float startY = Screen.height - 50f; 

        // Transform
        if (staminaBarLocation != null && playerCamera != null)
        {
            Vector3 screenPos = playerCamera.WorldToScreenPoint(staminaBarLocation.position);
            
            if (screenPos.z > 0)
            {
                startX = screenPos.x;
                startY = Screen.height - screenPos.y;
            }
        }

        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
        textStyle.alignment = TextAnchor.LowerLeft;
        textStyle.normal.textColor = Color.white;
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.fontSize = 16; 

        Rect textRect = new Rect(startX + maxBarWidth - 100f, startY - 26f, 100f, 20f);
        GUI.Label(textRect, "Sprint", textStyle);

        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.DrawTexture(new Rect(startX, startY, maxBarWidth, barThickness), Texture2D.whiteTexture);

        Color customBlue = new Color(47f / 255f, 106f / 255f, 192f / 255f);
        Color barColor = isExhausted ? Color.red : customBlue;
        
        GUI.color = barColor;

        float currentWidth = maxBarWidth * staminaPercent;
        Rect activeBarRect = new Rect(startX, startY, currentWidth, barThickness);
        GUI.DrawTexture(activeBarRect, Texture2D.whiteTexture);

        GUI.color = originalColor;
    }
}