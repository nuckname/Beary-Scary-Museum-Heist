using UnityEngine;

public class PlayerGrabController : MonoBehaviour
{
    [SerializeField] private Transform playerHand;
    private PlayerStealthController playerStealthController;

    public GameObject PickedUpObject { get; private set; }
    
    // Changed to an array to hold multiple IPickable scripts (This fixes the issue of only one IPickable being recognized when multiple are on the same object)
    public IPickable[] CurrentPickables { get; private set; }

    private void Awake()
    {
        playerStealthController = GetComponent<PlayerStealthController>();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("CanPickUp") && PickedUpObject == null)
        {
            IPickable[] pickables = hit.gameObject.GetComponents<IPickable>();
            
            if (pickables.Length > 0)
            {
                PickUpObject(hit.gameObject, pickables);
            }
        }
    }

    private void PickUpObject(GameObject obj, IPickable[] pickables)
    {
        PickedUpObject = obj;
        CurrentPickables = pickables;

        float totalWeight = 0f;

        // Loop through every IPickable script and call OnPickedUp
        foreach (var pickable in CurrentPickables)
        {
            pickable.OnPickedUp();
            
            // This is a problem becuase we are adding double the weight
            totalWeight += pickable.Weight;
        }

        // Attach to hand
        obj.transform.SetParent(playerHand);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        playerStealthController.walkSpeed -= totalWeight;
        playerStealthController.sprintSpeed -= totalWeight;
    }

    public void ReleaseObject()
    {
        if (PickedUpObject == null) return;

        float totalWeightToRestore = 0f;

        if (CurrentPickables != null)
        {
            // Loop through every IPickable script and call OnReleased
            foreach (var pickable in CurrentPickables)
            {
                pickable.OnReleased();
                totalWeightToRestore += pickable.Weight;
            }
        }

        PickedUpObject.transform.SetParent(null);
        
        playerStealthController.walkSpeed += totalWeightToRestore;
        playerStealthController.sprintSpeed += totalWeightToRestore;

        PickedUpObject = null;
        CurrentPickables = null;
    }
}