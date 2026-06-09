using UnityEngine;

public class DogWalkAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    public float walkSpeed = 5f;
    public float maxZRotation = 15f;
    public float maxYPosition = 3f;

    public bool isWalking = true; 

    private Vector3 startPos;
    private Quaternion startRot;
    private float animTimer = 0f;

    void Start()
    {
        // Store the initial position and rotation so we can animate relative to it
        startPos = transform.localPosition;
        startRot = transform.localRotation;
    }

    void Update()
    {
        if (!isWalking) return;

        animTimer += Time.deltaTime * walkSpeed;

        float zRot = Mathf.Sin(animTimer) * maxZRotation;
        
        transform.localRotation = startRot * Quaternion.Euler(0f, 0f, zRot);

        float yOffset = Mathf.Abs(Mathf.Sin(animTimer)) * maxYPosition;
        
        transform.localPosition = new Vector3(startPos.x, startPos.y + yOffset, startPos.z);
    }

    // Call this to snap the character back to a standing position
    public void ResetPosture()
    {
        transform.localPosition = startPos;
        transform.localRotation = startRot;
    }
}