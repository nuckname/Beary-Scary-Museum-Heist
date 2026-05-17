using UnityEngine;

public class CameraSweepState : CameraBaseState
{
    private Quaternion leftRotation;
    private Quaternion rightRotation;
    private Quaternion targetRotation;
    private float pauseTimer;
    private bool isWaiting;

    public override void EnterState(SecurityCameraController camera)
    {
        // Calculate the world rotations based on the camera's starting orientation
        Quaternion startRot = camera.transform.rotation;
        leftRotation = startRot * Quaternion.Euler(0, camera.leftAngle, 0);
        rightRotation = startRot * Quaternion.Euler(0, camera.rightAngle, 0);

        targetRotation = camera.startTurningRight ? rightRotation : leftRotation;
        
        isWaiting = false;
        pauseTimer = 0f;
    }

    public override void UpdateState(SecurityCameraController camera)
    {
        if (isWaiting)
        {
            pauseTimer += Time.deltaTime;
            if (pauseTimer >= camera.pauseTimeAtEdges)
            {
                // Done waiting, swap target and resume moving
                isWaiting = false;
                targetRotation = (targetRotation == rightRotation) ? leftRotation : rightRotation;
            }
        }
        else
        {
            // Rotate towards the target
            camera.cameraHead.rotation = Quaternion.RotateTowards(
                camera.cameraHead.rotation, 
                targetRotation, 
                camera.rotationSpeed * Time.deltaTime
            );

            // Check if we have arrived at the target angle
            if (Quaternion.Angle(camera.cameraHead.rotation, targetRotation) < 0.1f)
            {
                isWaiting = true;
                pauseTimer = 0f;
            }
        }
    }
}