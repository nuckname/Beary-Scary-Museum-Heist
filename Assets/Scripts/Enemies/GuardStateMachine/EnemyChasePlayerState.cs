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
       
            /*
            if (manager.isTutorial)
            {
                Vector3 spawnpoint = GameObject.FindGameObjectWithTag("SpawnPoint").transform.position;
                
                // Teleport the player
                other.transform.position = spawnpoint;
                
                manager.agent.ResetPath();
                
                manager.animator.SetBool("isHitPlayer", false);
                manager.animator.SetBool("isChasing", false);
                
                manager.SwitchState(manager.EnemyFollowPathState); 
                
                // Move guard to orginal positon
                manager.GetComponent<Transform>().position = new Vector3(113.010002f,2.51999998f,-68.7699966f);
                
                return;
            }
            */
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}