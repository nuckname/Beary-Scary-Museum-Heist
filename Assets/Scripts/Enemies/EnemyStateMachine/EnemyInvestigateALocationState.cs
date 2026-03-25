using UnityEngine;

public class EnemyInvestigateALocationState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager manager)
    {
        Debug.Log("State: Investigating!");

        if (manager.stateText != null)
        {
            manager.stateText.text = "!!!";
            manager.stateText.color = Color.yellow;
        }
    }

    public override void UpdateState(EnemyStateManager manager)
    {
        Vector3 targetLocation = manager.investigateTargetPosition;
        targetLocation.y = manager.transform.position.y; // Keep it on the same height plane

        // Rotate towards the suspicious location
        Vector3 direction = (targetLocation - manager.transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            manager.transform.rotation = Quaternion.RotateTowards(manager.transform.rotation, targetRotation, manager.turnSpeed * Time.deltaTime);
        }

        // Move towards the location
        manager.transform.position = Vector3.MoveTowards(manager.transform.position, targetLocation, manager.currentWalkSpeed * Time.deltaTime);

        // Did we arrive?
        if (Vector3.Distance(manager.transform.position, targetLocation) < 0.5f)
        {
            // We reached the target location and the player isn't here. 
            // Switch to Lost Player state to trigger the look-around behavior!
            manager.SwitchState(manager.EnemyLostPlayerState);
        }
    }

    public override void OnCollisionEnter2D(EnemyStateManager manager, Collision2D other)
    {
        // Collision logic
    }
}