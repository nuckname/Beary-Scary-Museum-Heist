using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class RoundGameOverState : RoundBaseState
{
    // When we reach the amount of rounds we call this to go to next level. can have multiple rounds a map.
    private TempScoreboard scoreboard;
    public override void EnterState(RoundStateManager manager)
    {
        Debug.Log("Round Over. Threshold Reached.");
        Debug.Log("Seen: " + RoundStateManager.AmountOfTimesPlayerSpottedByGuards);

        // Player also needs to enter?
        // Then we can switch back to About to start state.
        manager.currentRoundIndex++;
    }

    public override void UpdateState(RoundStateManager manager)
    {
        if(Input.GetKeyDown(KeyCode.E))
        {            
            //Reset score board
            scoreboard.isGameOver = false;

            // Index next scene
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1);
        }
    }
    public override void OnCollisionEnter(RoundStateManager manager, Collision collision) { }
    public override void OnCollisionExit(RoundStateManager manager, Collision collision) { }
    public override void OnTriggerEnter(RoundStateManager manager, Collider other) { }
    public override void OnTriggerExit(RoundStateManager manager, Collider other) { }
}