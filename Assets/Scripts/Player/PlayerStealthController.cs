using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStealthController : MonoBehaviour
{
    public float sneakSpeed;
    public float walkSpeed;
    public float sprintSpeed;

    public bool cannotMove = false;
    
    [Header("Stamina Settings")]
    public float maxStamina = 5f;
    public float staminaRegenRate = 1.5f;
    public float staminaDepleteRate = 1f;
    public float sneakStaminaDepleteRate = 2f;

    [Header("Footstep Settings")]
    public float walkStepInterval = 0.5f;
    public float sprintStepInterval = 0.3f;
    public float sneakStepInterval = 0.8f;
    private float stepTimer;

    [Header("UI Settings")]
    public Image staminaBarFill; 

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
        staminaBarFill = GameObject.FindGameObjectWithTag("SprintBarFill").GetComponent<Image>();
    }

    void Start()
    {
        currentStamina = maxStamina;
        lockedY = transform.position.y;
    }

    void Update()
    {
        if (cannotMove)
        {
            rb.linearVelocity = Vector3.zero;
            animator.SetBool("IsMoving", false);
            return;
        }
        
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        HandleSpeed();
        MovePlayer();
        HandleMouseRotation();
        
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        bool isMoving = input.sqrMagnitude > 0.01f; 
        animator.SetBool("IsMoving", isMoving);

        HandleFootsteps(isMoving);
        
        // Update the visual UI Canvas bar
        UpdateStaminaUI();
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
        bool isTryingToSneak = Input.GetKey(KeyCode.Mouse1) && isMoving && !isExhausted;

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
            
            currentStamina -= sneakStaminaDepleteRate * Time.deltaTime;

            if (currentStamina <= 0)
            {
                currentStamina = 0;
                isExhausted = true;
            }
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

    // Handles the modern Canvas UI updating
    void UpdateStaminaUI()
    {
        if (staminaBarFill != null)
        {
            staminaBarFill.fillAmount = currentStamina / maxStamina;
        }
    }
}