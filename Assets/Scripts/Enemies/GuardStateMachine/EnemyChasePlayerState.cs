using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (manager.playerTransform == null) return;

        manager.agent.SetDestination(manager.playerTransform.position);
    }
    public override void OnCollisionEnter(EnemyStateManager manager, Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // Hit animation
            manager.animator.SetBool("isHitPlayer", true);

            
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}