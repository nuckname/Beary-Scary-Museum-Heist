using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushInteractable : MonoBehaviour
{
    [Header("Tipping / Rotation Settings")]
    public Vector3 tipAxis = Vector3.right;
    public float tipSpeed = 25f;
    public float pointOfNoReturnAngle = 9f;
    public bool resetIfReleasedEarly = true;

    [Header("Pivot Points (Assign in Inspector)")]
    [Tooltip("Empty GameObject placed at the bottom edge you tip TOWARDS (positive).")]
    public Transform positivePivot;
    [Tooltip("Empty GameObject placed at the bottom edge you tip AWAY FROM (negative).")]
    public Transform negativePivot;

    [Header("Physics Push Settings (Post-Fall)")]
    public float pushForce = 5f;

    private Rigidbody rb;
    private Quaternion startRotation;
    private Vector3 startPosition;
    
    // We store the local offsets of the pivots so they never drift
    private Vector3 positivePivotLocalOffset;
    private Vector3 negativePivotLocalOffset;
    
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
        startPosition = transform.position; 
        
        // Calculate where the pivots are relative to the object's center
        if (positivePivot != null)
            positivePivotLocalOffset = transform.InverseTransformPoint(positivePivot.position);
        if (negativePivot != null)
            negativePivotLocalOffset = transform.InverseTransformPoint(negativePivot.position);
        
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    private void FixedUpdate()
    {
        HandleTipping();
        HandlePhysicsPush();
    }

    private void HandleTipping()
    {
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

        if (!hasFallen)
        {
            // 1. Calculate the raw rotation
            Quaternion tipRotation = Quaternion.Euler(tipAxis.normalized * (currentTipAngle * activeTipDirection));
            
            // 2. Identify which pivot we are rotating around based on direction
            Vector3 activeLocalOffset = activeTipDirection > 0 ? positivePivotLocalOffset : negativePivotLocalOffset;
            
            // 3. Find exactly where that pivot point is in the world right now (unrotated base)
            Vector3 basePivotWorld = startPosition + (startRotation * activeLocalOffset);
            
            // 4. Calculate the distance from the pivot to the object's center
            Vector3 dirFromPivotToCenter = startPosition - basePivotWorld;
            
            // 5. Rotate that distance vector by our tipping angle
            Vector3 rotatedDir = tipRotation * dirFromPivotToCenter;
            
            // 6. Apply flawless rotation around the pivot edge
            rb.MovePosition(basePivotWorld + rotatedDir);
            rb.MoveRotation(startRotation * tipRotation);
        }
    }

    private void TriggerFallOver()
    {
        hasFallen = true;
        isTipping = false; 
        
        rb.constraints = RigidbodyConstraints.None;

        Vector3 worldTipAxis = transform.TransformDirection(tipAxis);
        rb.AddTorque(worldTipAxis * activeTipDirection * 2f, ForceMode.VelocityChange);
    }

    private void HandlePhysicsPush()
    {
        if (currentPusher == null || !hasFallen) return;

        Vector3 forceDirection = transform.position - currentPusher.position;
        forceDirection.y = 0;
        
        if (forceDirection != Vector3.zero) 
        {
            forceDirection.Normalize();
        }

        rb.AddForce(forceDirection * pushForce, ForceMode.Force);
    }

    public void StartTipping(float direction)
    {
        if (hasFallen) return; 
        isTipping = true;
        activeTipDirection = direction;
    }

    public void StopTipping()
    {
        isTipping = false;
    }

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

    // Draws spheres in the editor so you can visually verify your pivot placement
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        if (positivePivot != null) Gizmos.DrawSphere(positivePivot.position, 0.1f);
        Gizmos.color = Color.red;
        if (negativePivot != null) Gizmos.DrawSphere(negativePivot.position, 0.1f);
    }
}