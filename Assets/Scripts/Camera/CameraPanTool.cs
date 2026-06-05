using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraPanTool : MonoBehaviour
{
    [Header("Panning Settings")]
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private bool enableEdgeScrolling = false;
    [SerializeField] private float edgeScrollSize = 20f;

    [Header("Drag Settings")]
    [SerializeField] private bool enableDragPan = true;
    [SerializeField] private KeyCode dragKey = KeyCode.Mouse2; // Middle mouse button
    [SerializeField] private float dragSpeed = 1f;

    [Header("Zoom Settings")]
    [SerializeField] private bool enableZoom = true;
    [SerializeField] private float zoomSpeed = 10f;
    [SerializeField] private float minZoomSize = 5f;
    [SerializeField] private float maxZoomSize = 20f;

    [Header("Limits")]
    [SerializeField] private bool enableLimits = true;
    [SerializeField] private Vector2 limitX = new Vector2(-50f, 50f);
    [SerializeField] private Vector2 limitZ = new Vector2(-50f, 50f);

    [Header("Auto-Pan to Transforms")]
    [SerializeField] private Transform targetA;
    [SerializeField] private Transform targetB;
    [SerializeField] private float autoPanSmoothTime = 0.3f; // The approximate time it takes to reach the target

    private Camera cam;
    private Vector3 dragOrigin;
    
    // State tracking for real-time panning
    private Transform activeTarget = null;
    private Vector3 panVelocity = Vector3.zero;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        // Example triggers
        if (Input.GetKeyDown(KeyCode.Alpha1)) PanToTargetA();
        if (Input.GetKeyDown(KeyCode.Alpha2)) PanToTargetB();

        // If we have an active target, handle the smooth damp and block manual control
        if (activeTarget != null)
        {
            HandleRealTimeAutoPan();
            return; 
        }

        HandleKeyboardAndEdgePanning();
        
        if (enableDragPan) HandleDragPanning();
        if (enableZoom) HandleZooming();
        if (enableLimits) ClampCameraPosition();
    }

    // --- Real-Time Auto-Panning Logic ---

    public void PanToTargetA() => activeTarget = targetA;
    public void PanToTargetB() => activeTarget = targetB;
    public void PanToSpecificTransform(Transform customTarget) => activeTarget = customTarget;

    /// <summary>
    /// Call this from elsewhere to manually break the camera's lock on a target.
    /// </summary>
    public void StopAutoPan() => activeTarget = null;

    private void HandleRealTimeAutoPan()
    {
        // Smoothly move towards the moving target's exact position this frame
        transform.position = Vector3.SmoothDamp(transform.position, activeTarget.position, ref panVelocity, autoPanSmoothTime);

        // Snap and release control when we get close enough to the target
        if (Vector3.Distance(transform.position, activeTarget.position) < 0.1f)
        {
            transform.position = activeTarget.position;
            activeTarget = null;
            panVelocity = Vector3.zero; // Reset velocity for the next pan
        }
    }

    // --- Manual Control Logic ---

    private void HandleKeyboardAndEdgePanning()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) moveDirection += Vector3.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) moveDirection += Vector3.back;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) moveDirection += Vector3.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) moveDirection += Vector3.right;

        if (enableEdgeScrolling)
        {
            if (Input.mousePosition.y >= Screen.height - edgeScrollSize) moveDirection += Vector3.forward;
            if (Input.mousePosition.y <= edgeScrollSize) moveDirection += Vector3.back;
            if (Input.mousePosition.x <= edgeScrollSize) moveDirection += Vector3.left;
            if (Input.mousePosition.x >= Screen.width - edgeScrollSize) moveDirection += Vector3.right;
        }

        transform.Translate(moveDirection.normalized * panSpeed * Time.deltaTime, Space.World);
    }

    private void HandleDragPanning()
    {
        if (Input.GetKeyDown(dragKey))
        {
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            
            if (groundPlane.Raycast(ray, out float entry))
            {
                dragOrigin = ray.GetPoint(entry);
            }
        }

        if (Input.GetKey(dragKey))
        {
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            
            if (groundPlane.Raycast(ray, out float entry))
            {
                Vector3 difference = dragOrigin - ray.GetPoint(entry);
                transform.position += difference * dragSpeed;
            }
        }
    }

    private void HandleZooming()
    {
        float scrollData = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollData) > 0.01f)
        {
            if (cam.orthographic)
            {
                cam.orthographicSize -= scrollData * zoomSpeed;
                cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoomSize, maxZoomSize);
            }
            else
            {
                Vector3 zoomDirection = transform.forward * scrollData * zoomSpeed;
                transform.position += zoomDirection;
            }
        }
    }

    private void ClampCameraPosition()
    {
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, limitX.x, limitX.y);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, limitZ.x, limitZ.y);
        transform.position = clampedPosition;
    }
}