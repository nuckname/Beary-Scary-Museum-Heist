using UnityEngine;

// https://gemini.google.com/app/4db71c22a502ce38?hl=en-AU
// AI gen https://gemini.google.com/share/e8c0047c52fb
public class CameraFollow : MonoBehaviour
{
    private Transform player;

    [Header("Current Camera Angle")]
    public Vector3 offset;
    public Vector3 normalRotationEuler;

    [Header("Saved Normal View Settings")] public Vector3 savedNormalOffset;
    public Vector3 savedNormalRotation;
    public bool lockYAxis = false;
    public float fixedYPosition = 0f;

    [Header("Saved Top-Down View Settings")]
    public bool useTopDownView = false;
    public Vector3 topDownOffset;
    public Vector3 topDownRotation;

    [Header("Camera Smoothing")]
    [Range(1f, 20f)]
    public float positionSmoothSpeed = 5f;
    [Range(1f, 20f)]
    public float rotationSmoothSpeed = 5f;

    private bool wasTopDownLastFrame = false;

    void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        
        if (p != null) 
        {
            player = p.transform;
        }
   
        offset = savedNormalOffset;
        normalRotationEuler = savedNormalRotation;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Detect if you right-clicked and the boolean changed this frame
        if (useTopDownView != wasTopDownLastFrame)
        {
            if (useTopDownView)
            {
                // Overwrite the live values with Top Down values
                offset = topDownOffset;
                normalRotationEuler = topDownRotation;
            }
            else
            {
                // Revert back to the saved Normal values
                offset = savedNormalOffset;
                normalRotationEuler = savedNormalRotation;
            }
            // Save the state so we don't trigger this overwrite every single frame
            wasTopDownLastFrame = useTopDownView; 
        }

        // Apply the live variables to the target
        Vector3 targetPosition = player.position + offset;
        Quaternion targetRotation = Quaternion.Euler(normalRotationEuler);

        if (!useTopDownView && lockYAxis)
        {
            targetPosition.y = fixedYPosition;
        }

        // Move the camera
        transform.position = Vector3.Lerp(transform.position, targetPosition, positionSmoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSmoothSpeed * Time.deltaTime);
    }
}