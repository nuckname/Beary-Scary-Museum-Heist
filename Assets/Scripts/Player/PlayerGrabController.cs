using System;
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

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("CanPickUp") && PickedUpObject == null)
        {
            IPickable[] pickables = other.gameObject.GetComponents<IPickable>();
        
            if (pickables.Length > 0)
            {
                if (pickables[0].CanBePickedUp && pickables[0].IsOnGround())
                {
                    PickUpObject(other.gameObject, pickables);
                }
            }
            else
            {
                Debug.LogError("pickables length is 0");
            }
        }
    }

    private void PickUpObject(GameObject obj, IPickable[] pickables)
    {
        PickedUpObject = obj;
        CurrentPickables = pickables;

        // Loop through every IPickable script and call OnPickedUp
        foreach (IPickable pickable in CurrentPickables)
        {
            pickable.OnPickedUp();
        }
        
        currentHeldWeight = 0f;
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        currentHeldWeight = rb.mass;
        
        playerFootstepNoise.SetWeightModifier(currentHeldWeight);

        // Store the object's original world scale before parenting
        Vector3 originalScale = obj.transform.localScale;
        
        // Attach to hand
        obj.transform.SetParent(playerHand);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        
        // Re-apply the scale so it doesn't distort
        obj.transform.localScale = originalScale;

        playerStealthController.walkSpeed -= currentHeldWeight;
        playerStealthController.sprintSpeed -= currentHeldWeight;
    }

    public void ReleaseObject()
    {
        if (PickedUpObject == null) return;

        if (CurrentPickables != null)
        {
            foreach (IPickable pickable in CurrentPickables)
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