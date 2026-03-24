using UnityEngine;

public class EnemyChasePlayerState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager manager)
    {
        // Set the speed to our faster chase speed
        manager.currentWalkSpeed = manager.chaseSpeed;

        if (manager.stateText != null)
        {
            manager.stateText.text = "CHASING!";
            manager.stateText.color = Color.red;
        }
        
        manager.currentWalkSpeed = manager.chaseSpeed;
    }

    public override void UpdateState(EnemyStateManager manager)
    {
        // If we somehow lost the reference to the player, do nothing
        if (manager.playerTransform == null) return;

        // Continuously get the player's CURRENT position
        Vector3 targetLocation = manager.playerTransform.position;
        targetLocation.y = manager.transform.position.y; // Keep it on the same height plane

        // Rotate towards the player
        Vector3 direction = (targetLocation - manager.transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            manager.transform.rotation = Quaternion.RotateTowards(manager.transform.rotation, targetRotation, manager.turnSpeed * Time.deltaTime);
        }

        // Move towards the player
        manager.transform.position = Vector3.MoveTowards(manager.transform.position, targetLocation, manager.currentWalkSpeed * Time.deltaTime);
    }

    public override void OnCollisionEnter2D(EnemyStateManager manager, Collision2D other)
    {
    }
}