using UnityEngine;

// https://gemini.google.com/app/4db71c22a502ce38?hl=en-AU
// AI gen https://gemini.google.com/share/e8c0047c52fb
public class CameraFollow : MonoBehaviour
{
    [Header("Target Setup")]
    public Transform player;

    [Header("Normal View Settings")]
    public Vector3 offset = new Vector3(0f, 2f, -10f);
    public bool lockYAxis = false;
    public float fixedYPosition = 0f;

    [Header("Top-Down View Settings")]
    [Tooltip("Toggle this to switch between normal and top-down view")]
    public bool useTopDownView = false;
    public Vector3 topDownOffset = new Vector3(0f, 15f, 0f); // High up, directly above the player
    public Vector3 topDownRotation = new Vector3(90f, 0f, 0f); // Looking straight down at the ground

    [Header("Camera Smoothing")]
    [Range(1f, 10f)]
    public float positionSmoothSpeed = 5f;
    [Range(1f, 10f)]
    public float rotationSmoothSpeed = 5f;

    private Quaternion normalRotation;

    void Start()
    {
        // Store the initial rotation of the camera to return to it when top-down is toggled off
        normalRotation = transform.rotation;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // 1. Determine target position and rotation based on the current mode
        Vector3 targetPosition;
        Quaternion targetRotation;

        if (useTopDownView)
        {
            // Top-Down Mode
            targetPosition = player.position + topDownOffset;
            targetRotation = Quaternion.Euler(topDownRotation);
        }
        else
        {
            // Normal Mode
            targetPosition = player.position + offset;
            targetRotation = normalRotation;

            // Apply Y-axis lock only in normal view (locking Y in top-down usually breaks it)
            if (lockYAxis)
            {
                targetPosition.y = fixedYPosition;
            }
        }

        // 2. Smoothly move the camera's position
        transform.position = Vector3.Lerp(transform.position, targetPosition, positionSmoothSpeed * Time.deltaTime);

        // 3. Smoothly adjust the camera's rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
    }
}