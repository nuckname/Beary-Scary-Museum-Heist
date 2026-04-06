using UnityEngine;

public class EnemyFollowPathState : EnemyBaseState
{
    private int targetWaypointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    public override void EnterState(EnemyStateManager manager)
    {
        isWaiting = false;
        waitTimer = 0f;
        manager.SetStateIcon(EnemyStateIcon.Hide);

        manager.agent.acceleration = 100f; 
        
        manager.agent.angularSpeed = manager.turnSpeed;

        manager.GuardStartMoving();
        //manager.GuardStopMoving();
      //  manager.agent.updateRotation = true; 
        manager.agent.speed = manager.guardPatrollSpeed;

        // Start heading to the first target
        if (manager.waypoints.Length > 0)
        {
            manager.agent.SetDestination(manager.waypoints[targetWaypointIndex]);
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
                
                // Wait is over, tell the agent to resume moving
                manager.agent.isStopped = false;
             //   manager.agent.updateRotation = true; // Let the NavMeshAgent control rotation again
                manager.agent.SetDestination(targetWaypoint); 
            }
            
            return; 
        }

        // Did we arrive? 
        if (!manager.agent.pathPending && manager.agent.remainingDistance <= manager.agent.stoppingDistance + 0.1f)
        {
            targetWaypointIndex = (targetWaypointIndex + 1) % manager.waypoints.Length;
            
            isWaiting = true; 
            manager.agent.isStopped = true; // Stop the agent from walking while waiting
         //   manager.agent.updateRotation = false; // Stop the agent from fighting our manual RotateTowards code

            manager.SetStateIcon(EnemyStateIcon.Hide);
        }
    }

    public void RotateTowards(EnemyStateManager manager, Vector3 targetPos)
    {
        Vector3 direction = (targetPos - manager.transform.position).normalized;
        direction.y = 0; // Flatten the Y axis so the guard doesn't tilt up/down

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            manager.transform.rotation = Quaternion.RotateTowards(manager.transform.rotation, targetRotation, manager.turnSpeed * Time.deltaTime);
        }
    }

    public override void OnCollisionEnter(EnemyStateManager manager, Collision other)
    {
    }
}