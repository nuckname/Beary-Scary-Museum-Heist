using UnityEngine;

public class EnemyLostPlayerState : EnemyBaseState
{
    private float lostTimer = 0f;
    private float timeToStandConfused = 0.5f;

    public override void EnterState(EnemyStateManager manager)
    {
        Debug.Log("State: Lost Player");
        lostTimer = 0f;

        if (manager.stateText != null)
        {
            manager.stateText.text = "Lost player...";
            manager.stateText.color = Color.black;
        }
    }

    public override void UpdateState(EnemyStateManager manager)
    {
        lostTimer += Time.deltaTime;

        // Slowly rotate head gets moved to here?

        // Once the timer is up, go back to patrolling
        if (lostTimer >= timeToStandConfused)
        {
            manager.SwitchState(manager.EnemyFollowPathState);
        }
    }

    public override void OnCollisionEnter2D(EnemyStateManager manager, Collision2D other)
    {
        
    }
}