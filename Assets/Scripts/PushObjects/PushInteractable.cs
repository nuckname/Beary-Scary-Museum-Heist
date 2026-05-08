using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushInteractable : MonoBehaviour
{
    [Header("Tipping / Rotation Settings")]
    public Vector3 tipAxis = Vector3.right;
    public float tipSpeed = 25f;
    public float pointOfNoReturnAngle = 9f;
    public bool resetIfReleasedEarly = true;

    [Header("Physics Push Settings (Post-Fall)")]
    [Tooltip("Force applied when using the PhysicsPush interaction.")]
    public float pushForce = 5f;

    private Rigidbody rb;
    private Quaternion startRotation;
    
    // State tracking
    private bool isTipping = false;
    private bool hasFallen = false;
    private float currentTipAngle = 0f;
    private float activeTipDirection = 1f;

    private Transform currentPusher;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startRotation = transform.rotation;
        
        // Freeze everything so it only moves via our tipping code until it falls
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void FixedUpdate()
    {
        HandleTipping();
        HandlePhysicsPush();
    }

    private void HandleTipping()
    {
        // If it has already fallen, we completely ignore rotation/tipping logic
        if (hasFallen) return;

        if (isTipping)
        {
            currentTipAngle += tipSpeed * Time.fixedDeltaTime;
            
            if (currentTipAngle >= pointOfNoReturnAngle)
            {
                TriggerFallOver();
                return; 
            }
        }
        else if (resetIfReleasedEarly && currentTipAngle > 0f)
        {
            currentTipAngle -= tipSpeed * Time.fixedDeltaTime;
            if (currentTipAngle <= 0f) currentTipAngle = 0f;
        }

        // Apply the rotation safely using Rigidbody
        if (!hasFallen)
        {
            Quaternion tipRotation = Quaternion.Euler(tipAxis.normalized * (currentTipAngle * activeTipDirection));
            rb.MoveRotation(startRotation * tipRotation);
        }
    }

    private void TriggerFallOver()
    {
        hasFallen = true;
        isTipping = false; // Stop tipping logic
        
        // Unfreeze physics so gravity and pushing can take over
        rb.constraints = RigidbodyConstraints.None;

        // Give it a tiny extra nudge so it falls cleanly
        Vector3 worldTipAxis = transform.TransformDirection(tipAxis);
        rb.AddTorque(worldTipAxis * activeTipDirection * 2f, ForceMode.VelocityChange);
    }

    private void HandlePhysicsPush()
    {
        // If no one is pushing, or if it hasn't fallen yet (it's frozen anyway), do nothing
        if (currentPusher == null || !hasFallen) return;

        // Calculate force direction away from the pusher, ignoring Y axis
        Vector3 forceDirection = transform.position - currentPusher.position;
        forceDirection.y = 0;
        
        if (forceDirection != Vector3.zero) 
        {
            forceDirection.Normalize();
        }

        // Push the unfrozen Rigidbody across the floor
        rb.AddForce(forceDirection * pushForce, ForceMode.Force);
    }


    public void StartTipping(float direction)
    {
        if (hasFallen) return; // Ignore if already on the floor
        isTipping = true;
        activeTipDirection = direction;
    }

    public void StopTipping()
    {
        isTipping = false;
    }

    // Called by the detector to start/stop free pushing
    public void StartPhysicsPush(Transform pusher)
    {
        currentPusher = pusher;
    }

    public void StopPhysicsPush(Transform pusher)
    {
        if (currentPusher == pusher)
        {
            currentPusher = null;
        }
    }
}