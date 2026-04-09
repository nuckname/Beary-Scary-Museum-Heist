using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class RoundInProgressState : RoundBaseState
{
    public override void EnterState(RoundStateManager manager)
    {
        Debug.Log("Round is in progress.");

        //manager.timer.isPaused = false;

        manager.MakeGuardsStartMoving();
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
        HandlePlayerCollision(manager, other);

        HandleArtifactCollision(manager, other);
    }

    private static void HandleArtifactCollision(RoundStateManager manager, Collider other)
    {
        if (other.gameObject.CompareTag("CanPickUp"))
        {
            IArtifact artifact = other.GetComponent<IArtifact>();
        
            if (artifact != null)
            {
                if (!artifact.hasBeenUsed)
                {
                    manager.currentArtifacts++;
                    
                    artifact.hasBeenUsed = true; 
                    
                    manager.UpdateUI();
                }
            
                if (manager.currentArtifacts >= manager.amountOfArtifactsToCompleteLevel)
                {
                    manager.SwitchState(manager.GameOverState);
                }
            }
        }
    }

    private static void HandlePlayerCollision(RoundStateManager manager, Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if(manager.currentArtifacts > manager.amountOfArtifactsToCompleteLevel)
            {
                manager.SwitchState(manager.GameOverState);
            }
        }
    }

    public override void OnTriggerExit(RoundStateManager manager, Collider other)
    {
    }  
}