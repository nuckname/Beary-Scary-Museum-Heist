using UnityEngine;

public class EnemyFollowPathState : EnemyBaseState
{
    private int targetWaypointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    public override void EnterState(EnemyStateManager manager)
    {
        // Optional: Find the closest waypoint here so they don't walk through walls to get to waypoint[0]
        isWaiting = false;
        waitTimer = 0f;

        if (manager.stateText != null)
        {
            manager.stateText.text = "Patrolling";
            manager.stateText.color = Color.green;
        }
    }

    public override void UpdateState(EnemyStateManager manager)
    {
        if (manager.waypoints.Length == 0) return;

        Vector3 targetWaypoint = manager.waypoints[targetWaypointIndex];

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            
            // Turn to face the next waypoint while waiting
            RotateTowards(manager, targetWaypoint);

            if (waitTimer >= manager.waitTime)
            {
                isWaiting = false;
                waitTimer = 0f;

                if (manager.stateText != null)
                {
                    manager.stateText.text = "Patrolling";
                }
            }
            
            // Stop updating movement while waiting
            return; 
        }

        RotateTowards(manager, targetWaypoint);

        // Move towards target
        manager.transform.position = Vector3.MoveTowards(manager.transform.position, targetWaypoint, manager.walkSpeed * Time.deltaTime);

        // Check if arrived
        if (Vector3.Distance(manager.transform.position, targetWaypoint) < 0.1f)
        {
            targetWaypointIndex = (targetWaypointIndex + 1) % manager.waypoints.Length;
            isWaiting = true; // Trigger wait for next frame

            if (manager.stateText != null)
            {
                manager.stateText.text = "Waiting";
            }
        }
    }

    // This is public so we can randomly look around
    public void RotateTowards(EnemyStateManager manager, Vector3 targetPos)
    {
        Vector3 direction = (targetPos - manager.transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            manager.transform.rotation = Quaternion.RotateTowards(manager.transform.rotation, targetRotation, manager.turnSpeed * Time.deltaTime);
        }
    }


    public override void OnCollisionEnter2D(EnemyStateManager manager, Collision2D other)
    {
        
    }
}