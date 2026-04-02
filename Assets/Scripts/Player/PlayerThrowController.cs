using UnityEngine;

// help https://www.youtube.com/watch?v=K4DMCseZA08
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
        lineRenderer.colorGradient = lineGradient;
        
        // Only apply the material if you assigned one in the Inspector
        if (lineMaterial != null)
        {
            lineRenderer.material = lineMaterial;
        }
        
        // Hide the line to start
        lineRenderer.enabled = false; 
    }

    private void Update()
    {
        if (grabController.PickedUpObject == null)
        {
            isCharging = false;
            currentThrowForce = minThrowForce;
            lineRenderer.enabled = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            // Check if the held object can be thrown before starting the charge
            
            IThrowableItem throwable = grabController.PickedUpObject.GetComponent<IThrowableItem>();
            
            if (throwable != null && !throwable.CanThrowItem)
            {
                return;
            }
            
            isCharging = true;
            currentThrowForce = minThrowForce;
            lineRenderer.enabled = true; 
        }

        if (isCharging && Input.GetMouseButton(0))
        {
            currentThrowForce += chargeRate * Time.deltaTime;
            currentThrowForce = Mathf.Clamp(currentThrowForce, minThrowForce, maxThrowForce);
            
            DrawTrajectory(); 
        }

        if (isCharging && Input.GetMouseButtonUp(0))
        {
            ThrowObject();
        }
    }

    // AI https://gemini.google.com/share/3ac24891bcfb
    private void DrawTrajectory()
    {
        // Start the line at the exact position of the held object
        Vector3 startPosition = grabController.PickedUpObject.transform.position; 
        Vector3 throwDirection = transform.forward + (Vector3.up * 0.5f);

        float mass = 1f;
        Rigidbody rb = grabController.PickedUpObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Prevents divide by zero errors
            mass = Mathf.Max(rb.mass, 0.01f);
        }

        float effectiveForce = currentThrowForce / mass; 
        Vector3 velocity = throwDirection.normalized * effectiveForce;

        for (int i = 0; i < linePoints; i++)
        {
            float time = i * timeBetweenPoints;
            Vector3 pointPosition = startPosition + velocity * time + 0.5f * Physics.gravity * (time * time);
            
            lineRenderer.SetPosition(i, pointPosition);
        }
    }

    private void ThrowObject()
    {
        GameObject objectToThrow = grabController.PickedUpObject;
    
        // Release the object first (calls IPickable.OnReleased)
        grabController.ReleaseObject();

        Vector3 throwDirection = transform.forward + (Vector3.up * 0.5f);
        Rigidbody boxRb = objectToThrow.GetComponent<Rigidbody>();
    
        float effectiveForce = currentThrowForce / Mathf.Max(boxRb.mass, 0.01f); 
        Vector3 calculatedVelocity = throwDirection.normalized * effectiveForce;

        // Check for our interface
        IThrowableItem throwable = objectToThrow.GetComponent<IThrowableItem>();
    
        if (throwable != null)
        {
            throwable.OnThrown(calculatedVelocity);
        }
        
        isCharging = false;
        currentThrowForce = minThrowForce;
        lineRenderer.enabled = false; 
    }
}