using UnityEngine;

public class RoundGameOverState : RoundBaseState
{
    public override void EnterState(RoundStateManager manager)
    {
        Debug.Log("Round Over. Threshold Reached.");
        
        manager.timer.isPaused = true;
        
        // freeze game
        // show score board UI?
    }

    public override void UpdateState(RoundStateManager manager)
    {
    }

    public override void OnCollisionEnter(RoundStateManager manager, Collision collision)
    {
    }

    public override void OnCollisionExit(RoundStateManager manager, Collision collision)
    {
    }

    public override void OnTriggerEnter(RoundStateManager manager, Collider other)
    {
    }

    public override void OnTriggerExit(RoundStateManager manager, Collider other)
    {
    }
}