using UnityEngine;

//AI gen https://gemini.google.com/share/e8c0047c52fb
public class CameraFollow : MonoBehaviour
{
    [Header("Target Setup")]
    public Transform player;
    public Vector3 offset = new Vector3(0f, 2f, -10f); // Adjust this to set distance from player

    [Header("Camera Settings")]
    [Range(1f, 10f)]
    public float smoothSpeed = 5f; // How quickly the camera catches up
    
    // Optional: Lock the Y axis if you only want it to scroll left/right
    public bool lockYAxis = false;
    public float fixedYPosition = 0f;

    void LateUpdate()
    {
        if (player == null) return;

        // Calculate the target position based on player position and offset
        Vector3 targetPosition = player.position + offset;

        // If we want a strict side-scroller, we can lock the Y axis
        if (lockYAxis)
        {
            targetPosition.y = fixedYPosition;
        }

        // Smoothly move the camera towards the target position
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}