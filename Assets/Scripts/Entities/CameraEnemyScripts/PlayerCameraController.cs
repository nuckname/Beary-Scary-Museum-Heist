using UnityEngine;

[RequireComponent(typeof(PlayerStealthController))]
[RequireComponent(typeof(Rigidbody))]
public class PlayerCameraController : MonoBehaviour
{
    [Header("References")]
    public CameraFollow cameraFollowScript;
    
    private PlayerStealthController stealthController;
    private Rigidbody rb;
    private Animator animator;

    private void Awake()
    {
        stealthController = GetComponent<PlayerStealthController>();
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();

        if (cameraFollowScript == null && Camera.main != null)
        {
            cameraFollowScript = Camera.main.GetComponent<CameraFollow>();
        }
    }

    private void Update()
    {
        if (cameraFollowScript == null) return;

        // Listen for Right-Click (Hold down to view, release to return)
        if (Input.GetMouseButtonDown(1))
        {
            ToggleTopDownView(true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            ToggleTopDownView(false);
        }
    }

    private void ToggleTopDownView(bool state)
    {
        cameraFollowScript.SetTopDownMode(state);

        if (state)
        {
            stealthController.enabled = false;

            rb.linearVelocity = Vector3.zero;

            // Force the movement animation to stop
            if (animator != null)
            {
                animator.SetBool("IsMoving", false);
            }
        }
        else
        {
            stealthController.enabled = true;
        }
    }
}