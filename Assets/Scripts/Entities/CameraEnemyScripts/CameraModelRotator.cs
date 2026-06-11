using UnityEngine;
/// <summary>
/// Rotate the physical model alined with the max and min angles specified in the SecurityCameraController.
/// </summary>
public class CameraModelRotator : MonoBehaviour
{
    public SecurityCameraController controller;

    private float currentYRotation;
    private int direction; // 1 for right, -1 for left
    private float pauseTimer;

    private void Start()
    {
        // Initialize starting direction and current rotation
        direction = controller.startTurningRight ? 1 : -1;
        
        // Get current local rotation and normalize it to -180 to 180 degrees
        currentYRotation = transform.localEulerAngles.y;
        if (currentYRotation > 180f) currentYRotation -= 360f;
    }

    private void Update()
    {
        // Guard clause: Only rotate if we are currently sweeping
        if (controller.currentStateName != "CameraSweepState") 
            return;

        if (pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
            return;
        }

        float step = controller.rotationSpeed * Time.deltaTime;
        currentYRotation += step * direction;

        // Check if we hit the right boundary
        if (direction == 1 && currentYRotation >= controller.rightAngle)
        {
            currentYRotation = controller.rightAngle;
            direction = -1; // Reverse direction
            pauseTimer = controller.pauseTimeAtEdges;
        }
        
        // Check if we hit the left boundary
        else if (direction == -1 && currentYRotation <= controller.leftAngle)
        {
            currentYRotation = controller.leftAngle;
            direction = 1; // Reverse direction
            pauseTimer = controller.pauseTimeAtEdges;
        }

        // Apply the rotation on the Y axis (Local rotation so it works regardless of parent rotation)
        transform.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);
    }
}