using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class RoundGameOverState : RoundBaseState
{
    private TempScoreboard scoreboard;
    public override void EnterState(RoundStateManager manager)
    {
        Debug.Log("Round Over. Threshold Reached.");
        Debug.Log("Seen: " + RoundStateManager.AmountOfTimesPlayerSpottedByGuards);
        
        manager.timer.isPaused = true;

        // Find the temp scoreboard in the scene and activate it
        scoreboard = Object.FindObjectOfType<TempScoreboard>();
        if (scoreboard != null)
        {
            scoreboard.isGameOver = true;
        }
        else
        {
            Debug.LogWarning("TempScoreboard script not found in the scene!");
        }
        
        // Lets just tp the player for now
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