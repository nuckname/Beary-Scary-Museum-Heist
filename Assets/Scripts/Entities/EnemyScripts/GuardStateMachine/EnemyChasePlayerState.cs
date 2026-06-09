using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Defines the AI behavior during active pursuit of the player. 
/// Continuously routes the NavMeshAgent to the player's position and handles the catch condition upon collision to reset the scene.
/// </summary>
public class EnemyChasePlayerState : EnemyBaseState
{
    public override void EnterState(EnemyStateManager manager)
    {
        manager.SetStateIcon(EnemyStateIcon.ChasingPlayer);
        manager.GuardStartMoving();
        manager.animator.SetBool("isChasing", true);
        manager.agent.speed = manager.guardChaseSpeed;
    }

    public override void UpdateState(EnemyStateManager manager)
    {
        if (manager.sensors.playerTransform != null)
        {
            manager.agent.SetDestination(manager.sensors.playerTransform.position);
        }
    }

    public override void OnCollisionEnter(EnemyStateManager manager, Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Stop the guard from sliding while playing the attack animation
            manager.agent.isStopped = true; 
            
            manager.animator.SetBool("isChasing", false);
            //manager.animator.SetBool("isHitPlayer", true);
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            
        }
    }
}