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

    private float currentStamina;
    private bool isExhausted = false;

    public Rigidbody rb;
    private Vector3 moveDirection;
    private float currentSpeed;
    private float lockedY;

    private Camera mainCamera;
    private CameraFollow cameraFollow; 

    [SerializeField] private bool displayDebugSprintBar = true; 

    private void Awake()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        cameraFollow = mainCamera.GetComponent<CameraFollow>();
    }

    void Start()
    {
        currentStamina = maxStamina;
        lockedY = transform.position.y;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (cameraFollow != null)
            {
                cameraFollow.useTopDownView = !cameraFollow.useTopDownView;
            }
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        HandleSpeed();
        MovePlayer();
        HandleMouseRotation();
    }

    void HandleSpeed()
    {
        // Stop movement completely if the camera is in top-down mode
        if (cameraFollow != null && cameraFollow.useTopDownView)
        {
            currentSpeed = 0f;
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
        rb.linearVelocity = moveDirection * currentSpeed;

        Vector3 pos = transform.position;
        pos.y = lockedY;
        transform.position = pos;
    }

    void HandleMouseRotation()
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
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
        if (!displayDebugSprintBar) return;
        
        Color originalColor = GUI.color;
        float staminaPercent = currentStamina / maxStamina;

        float maxBarWidth = 400f; 
        float barThickness = 24f; 
        float startX = (Screen.width / 2f) - (maxBarWidth / 2f);
        float startY = Screen.height - 50f; 

        GUIStyle textStyle = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.LowerLeft,
            fontStyle = FontStyle.Bold,
            fontSize = 16
        };
        textStyle.normal.textColor = Color.white;

        Rect textRect = new Rect(startX + maxBarWidth - 100f, startY - 26f, 100f, 20f);
        GUI.Label(textRect, "Sprint", textStyle);

        GUI.color = new Color(0, 0, 0, 0.6f);
        GUI.DrawTexture(new Rect(startX, startY, maxBarWidth, barThickness), Texture2D.whiteTexture);

        Color rainbowColor = Color.HSVToRGB(Mathf.Repeat(Time.time * 0.5f, 1f), 1f, 1f);
        GUI.color = isExhausted ? Color.red : rainbowColor;

        float currentWidth = maxBarWidth * staminaPercent;
        Rect activeBarRect = new Rect(startX, startY, currentWidth, barThickness);
        GUI.DrawTexture(activeBarRect, Texture2D.whiteTexture);

        GUI.color = originalColor;
    }
}