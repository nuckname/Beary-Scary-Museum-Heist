using UnityEngine;

public class CameraAlarmState : CameraBaseState
{
    public override void EnterState(SecurityCameraController camera)
    {
        Debug.Log("Camera Alarm Triggered!");
        
        if (camera.noiseEmitter != null)
        {
            camera.noiseEmitter.EmitNoise(camera.alarmNoiseRadius, NoiseType.Item);
        }
    }

    public override void UpdateState(SecurityCameraController camera)
    {
        if (camera.playerTransform != null)
        {
            Vector3 directionToPlayer = camera.playerTransform.position - camera.cameraHead.position;
            directionToPlayer.y = 0;
            
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                
                camera.cameraHead.rotation = Quaternion.RotateTowards(
                    camera.cameraHead.rotation, 
                    lookRotation, 
                    camera.rotationSpeed * 3f * Time.deltaTime 
                );
            }
        }
    }
}