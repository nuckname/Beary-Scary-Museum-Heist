using UnityEngine;

public class PlayerGrabController : MonoBehaviour
{
    [SerializeField] private Transform playerHand;
    private PlayerStealthController playerStealthController;
    private PlayerFootstepNoise playerFootstepNoise;
    
    public GameObject PickedUpObject { get; private set; }
    
    // Changed to an array to hold multiple IPickable scripts
    // (This fixes the issue of only one IPickable being recognized when multiple are on the same object)
    public IPickable[] CurrentPickables { get; private set; }

    private float currentHeldWeight = 0f;
    
    private void Awake()
    {
        playerStealthController = GetComponent<PlayerStealthController>();
        playerFootstepNoise = GetComponentInChildren<PlayerFootstepNoise>(); 
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

        // Loop through every IPickable script and call OnPickedUp
        foreach (var pickable in CurrentPickables)
        {
            pickable.OnPickedUp();
        }
        
        currentHeldWeight = 0f;
        
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        currentHeldWeight = rb.mass;
        

        playerFootstepNoise.SetWeightModifier(currentHeldWeight);
        
        // Attach to hand
        obj.transform.SetParent(playerHand);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        playerStealthController.walkSpeed -= currentHeldWeight;
        playerStealthController.sprintSpeed -= currentHeldWeight;
    }

    public void ReleaseObject()
    {
        if (PickedUpObject == null) return;

        if (CurrentPickables != null)
        {
            foreach (var pickable in CurrentPickables)
            {
                pickable.OnReleased();
            }
        }

        PickedUpObject.transform.SetParent(null);
        
        playerStealthController.walkSpeed += currentHeldWeight;
        playerStealthController.sprintSpeed += currentHeldWeight;
  
        playerFootstepNoise.SetWeightModifier(0f);
      

        PickedUpObject = null;
        CurrentPickables = null;
        currentHeldWeight = 0f;
    }
}