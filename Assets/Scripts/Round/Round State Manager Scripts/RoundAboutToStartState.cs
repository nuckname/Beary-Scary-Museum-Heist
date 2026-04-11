using UnityEngine;

public class RoundAboutToStartState : RoundBaseState
{
    public override void EnterState(RoundStateManager manager)
    {
        Debug.Log("Round is about to start. you are in green zone.");
        
        manager.LoadRound(manager.currentRoundIndex);
        manager.currentRoundIndex++;

        manager.ResetArtifacts();
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

    // This is called by DropOffZoneListener.cs
    public override void OnTriggerExit(RoundStateManager manager, Collider other)
    {
        if (other.CompareTag("Player"))
        {
            manager.SwitchState(manager.InProgressState);
        }
    }
}