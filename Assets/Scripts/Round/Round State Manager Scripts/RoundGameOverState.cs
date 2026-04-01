using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class RoundGameOverState : RoundBaseState
{
    public override void EnterState(RoundStateManager manager)
    {
        Debug.Log("Round Over. Threshold Reached.");
        Debug.Log("Seen: " + RoundStateManager.AmountOfTimesPlayerSpottedByGuards);
        
        manager.timer.isPaused = true;

        // Find the temp scoreboard in the scene and activate it
        TempScoreboard scoreboard = Object.FindObjectOfType<TempScoreboard>();
        if (scoreboard != null)
        {
            scoreboard.isGameOver = true;
        }
        else
        {
            Debug.LogWarning("TempScoreboard script not found in the scene!");
        }
    }

    public override void UpdateState(RoundStateManager manager) { }
    public override void OnCollisionEnter(RoundStateManager manager, Collision collision) { }
    public override void OnCollisionExit(RoundStateManager manager, Collision collision) { }
    public override void OnTriggerEnter(RoundStateManager manager, Collider other) { }
    public override void OnTriggerExit(RoundStateManager manager, Collider other) { }
}