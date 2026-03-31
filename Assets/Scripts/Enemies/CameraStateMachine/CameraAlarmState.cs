using UnityEngine;
using System.Collections;

public class CameraAlarmState : CameraBaseState
{
    private Coroutine alarmCoroutine;

    public override void EnterState(SecurityCameraController camera)
    {
        Debug.Log("Camera Alarm Triggered!");
        
        // Start the repeating alarm coroutine and save the reference
        alarmCoroutine = camera.StartCoroutine(AlarmRoutine(camera));
    }

    public override void UpdateState(SecurityCameraController camera)
    {
        // Track the player with the camera head
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

    public override void ExitState(SecurityCameraController camera)
    {
        Debug.Log("Player escaped! Stopping alarm.");
        if (alarmCoroutine != null)
        {
            camera.StopCoroutine(alarmCoroutine);
        }
    }

    private IEnumerator AlarmRoutine(SecurityCameraController camera)
    {
        // loop forever until StopCoroutine is called
        while (true) 
        {
            if (camera.noiseEmitter != null)
            {
                camera.noiseEmitter.EmitNoise(camera.alarmNoiseRadius, NoiseType.Player);
            }
            
            yield return new WaitForSeconds(camera.alarmBeepInterval);
        }
    }
}