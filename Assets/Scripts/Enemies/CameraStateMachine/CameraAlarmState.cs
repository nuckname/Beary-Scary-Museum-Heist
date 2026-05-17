using UnityEngine;

public class CameraAlarmState : CameraBaseState
{
    public override void EnterState(SecurityCameraController camera)
    {
        Debug.Log("Camera Alarm Triggered!");
        
        // Trigger the alarm on the reusable component
        if (camera.alarmComponent != null)
        {
            camera.alarmComponent.StartAlarm();
        }

        GameObject[] guards = GameObject.FindGameObjectsWithTag("Enemy");
        
        foreach (GameObject guard in guards)
        {
            EnemyStateManager enemyManager = guard.GetComponent<EnemyStateManager>();
            
            if (enemyManager != null)
            {
                // Only send the guard to investigate if they aren't already busy chasing the player or stunned
                if (enemyManager.EnemyCurrentState != enemyManager.EnemyChasePlayerState && 
                    enemyManager.EnemyCurrentState != enemyManager.EnemyStunnedState)
                {
                    // Set the target to the camera's location and switch to investigate state
                    enemyManager.investigateTargetPosition = camera.transform.position;
                    enemyManager.SwitchState(enemyManager.EnemyInvestigateState);
                    
                    camera.SwitchState(camera.SweepState);
                }
            }
        }
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
        
        // Stop the alarm on the reusable component
        if (camera.alarmComponent != null)
        {
            camera.alarmComponent.StopAlarm();
        }
    }
}