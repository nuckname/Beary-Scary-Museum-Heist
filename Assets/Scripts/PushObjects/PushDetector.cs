using UnityEngine;

// https://www.youtube.com/watch?v=3BOn2gs7z04
// https://www.youtube.com/watch?v=HKLwD3NC8Ro

[RequireComponent(typeof(Collider))]
public class PushDetector : MonoBehaviour
{
    public enum InteractionType 
    { 
        RotatePositive, 
        RotateNegative, 
        PhysicsPush 
    }

    public PushInteractable mainScript;

    public InteractionType interactionType = InteractionType.RotatePositive;

    [Header("Filtering")]
    public string pusherTag = "Player";
    public LayerMask pushableLayers;

    private void Awake()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsValidPusher(other.gameObject)) return;

        switch (interactionType)
        {
            case InteractionType.RotatePositive:
                mainScript.StartTipping(1f);
                break;
            case InteractionType.RotateNegative:
                mainScript.StartTipping(-1f);
                break;
            case InteractionType.PhysicsPush:
                mainScript.StartPhysicsPush(other.transform);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsValidPusher(other.gameObject)) return;

        switch (interactionType)
        {
            case InteractionType.RotatePositive:
            case InteractionType.RotateNegative:
                mainScript.StopTipping();
                break;
            case InteractionType.PhysicsPush:
                mainScript.StopPhysicsPush(other.transform);
                break;
        }
    }

    private bool IsValidPusher(GameObject obj)
    {
        bool matchesTag = !string.IsNullOrEmpty(pusherTag) && obj.CompareTag(pusherTag);
        
        // AI short hand
        // Just checks if the object's layer is included in the pushableLayers mask
        bool matchesLayer = (pushableLayers.value & (1 << obj.layer)) != 0;
        
        return matchesTag || matchesLayer;
    }
}