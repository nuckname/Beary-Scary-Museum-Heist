using UnityEngine;

public class RoundAboutToStartState : RoundBaseState
{
    public override void EnterState(RoundStateManager manager)
    {
        // Code to run once when the prep-phase/countdown starts
    }

    public override void UpdateState(RoundStateManager manager)
    {
        // Code to run every frame during the prep-phase
        // e.g., countdown timers, waiting for players to ready up
        
        // Example transition:
        // if (timer <= 0) manager.SwitchState(manager.InProgressState);
    }
}