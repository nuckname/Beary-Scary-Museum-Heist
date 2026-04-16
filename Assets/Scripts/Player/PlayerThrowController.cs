using UnityEngine;

// help https://www.youtube.com/watch?v=K4DMCseZA08
// AI help
[RequireComponent(typeof(LineRenderer))]
public class PlayerThrowController : MonoBehaviour
{
    [Header("Throw Settings")]
    [SerializeField] private float minThrowForce = 2f;
    [SerializeField] private float maxThrowForce = 15f;
    [SerializeField] private float chargeRate = 15f;

    [Header("Trajectory Line Settings")]
    [SerializeField] private int linePoints = 25;
    [SerializeField] private float timeBetweenPoints = 0.1f;
    
    [Space(10)] // Adds a little gap in the Inspector for organization
    [SerializeField] private float startWidth = 0.08f;
    [SerializeField] private float endWidth = 0.02f;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private Gradient lineGradient; // Controls both Color and Transparency (Alpha)

    [SerializeField] private LayerMask collisionLayers;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    
    private float currentThrowForce;
    private bool isCharging;

    private PlayerGrabController grabController;
    private LineRenderer lineRenderer;

    private void Awake()
    {
        grabController = GetComponent<PlayerGrabController>();
        lineRenderer = GetComponent<LineRenderer>();
        
        currentThrowForce = minThrowForce;
        
        // --- Setup LineRenderer Visuals ---
        lineRenderer.positionCount = linePoints;
        lineRenderer.startWidth = startWidth;
        lineRenderer.endWidth = endWidth;
 
        // Fixes that for some reason we arent changing the colour
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        
        // Hide the line to start
        lineRenderer.enabled = false; 
    }

    private void Update()
    {
        GameObject topObject = grabController.GetTopObject();

        if (topObject == null)
        {
            isCharging = false;
            currentThrowForce = minThrowForce;
            lineRenderer.enabled = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Check if the held object can be thrown before starting the charge
            
            IThrowableItem throwable = topObject.GetComponent<IThrowableItem>();
            
            if (throwable != null && !throwable.CanThrowItem)
            {
                return;
            }
            

            print("Animating throw false");
            //animator.SetBool("HasThrown", false);
         

            isCharging = true;
            currentThrowForce = minThrowForce;
            lineRenderer.enabled = true; 
        }

        if (isCharging && Input.GetMouseButton(0))
        {
            currentThrowForce += chargeRate * Time.deltaTime;
            currentThrowForce = Mathf.Clamp(currentThrowForce, minThrowForce, maxThrowForce);
            
           // DrawTrajectory(); 
        }

        if (isCharging && Input.GetMouseButtonUp(0))
        {
            print("Animating throw false");
            
            //animator.SetBool("HasThrown", false);
            ThrowObject();
        }
    }
    
    private void LateUpdate()
    {
        if (isCharging && grabController.GetTopObject() != null)
        {
            DrawTrajectory(); 
        }
    }

    // AI https://gemini.google.com/share/3ac24891bcfb
    private void DrawTrajectory()
    {
        GameObject topObject = grabController.GetTopObject();

        // Start the line at the exact position of the held object
        Vector3 startPosition = topObject.transform.position; 
        Vector3 throwDirection = transform.forward + (Vector3.up * 0.5f);

        float mass = 1f;
        Rigidbody rb = topObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Prevents divide by zero errors
            mass = Mathf.Max(rb.mass, 0.01f);
        }

        // Reduce thrown distance based on what Item we are throwing
        float typeMultiplier = 1f;
        IThrowableItem throwable = topObject.GetComponent<IThrowableItem>();
        if (throwable != null && throwable.ItemType == ItemType.Artifact)
        {
            typeMultiplier = 0.25f;
        }

        float effectiveForce = (currentThrowForce / mass) * typeMultiplier; 
        Vector3 velocity = throwDirection.normalized * effectiveForce;

        // Reset the line length just in case it was shortened in a previous frame
        lineRenderer.positionCount = linePoints;
        
        Vector3 previousPosition = startPosition;
        lineRenderer.SetPosition(0, previousPosition);

        // Loop through points, starting at index 1
        for (int i = 1; i < linePoints; i++)
        {
            float time = i * timeBetweenPoints;
            Vector3 currentPosition = startPosition + velocity * time + 0.5f * Physics.gravity * (time * time);
            
            // Draw a line from the last point to this new point to check for walls
            Vector3 segmentDirection = currentPosition - previousPosition;
            float segmentDistance = segmentDirection.magnitude;

            if (Physics.Raycast(previousPosition, segmentDirection.normalized, out RaycastHit hit, segmentDistance, collisionLayers))
            {
                // Cut the LineRenderer short at this index
                lineRenderer.positionCount = i + 1; 
                
                // Snap the final point to exactly where the wall was hit
                lineRenderer.SetPosition(i, hit.point); 
                
                // Stop calculating points since we hit a wall
                break; 
            }

            // If no wall was hit, place the point normally
            lineRenderer.SetPosition(i, currentPosition);
            previousPosition = currentPosition;
        }
    }

    private void ThrowObject()
    {
        GameObject objectToThrow = grabController.GetTopObject();
    
        // Release the object first (calls IPickable.OnReleased)
        grabController.ReleaseTopObject();

        Vector3 throwDirection = transform.forward + (Vector3.up * 0.5f);
        Rigidbody boxRb = objectToThrow.GetComponent<Rigidbody>();
    
        // Check for our interface
        IThrowableItem throwable = objectToThrow.GetComponent<IThrowableItem>();

        float typeMultiplier = 1f;
        if (throwable != null && throwable.ItemType == ItemType.Artifact)
        {
            typeMultiplier = 0.25f; // Reduces throwing effect by 75%, leaving 25%
        }

        float effectiveForce = (currentThrowForce / Mathf.Max(boxRb.mass, 0.01f)) * typeMultiplier; 
        Vector3 calculatedVelocity = throwDirection.normalized * effectiveForce;
    
        if (throwable != null)
        {
            throwable.OnThrown(calculatedVelocity);
        }

        print("Animating throw true");
            //animator.SetBool("HasThrown", true);
       
        
        isCharging = false;
        currentThrowForce = minThrowForce;
        lineRenderer.enabled = false; 
    }
}