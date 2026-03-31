using UnityEngine;

// this is not mine https://www.patreon.com/posts/how-to-make-144078368
public class Detector : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public LayerMask obstructionMask;

    [Header("Debug Settings")]
    public bool showDebugRay = true;
    public Color rayColorHit = Color.red;
    public Color rayColorNoHit = Color.green;

    private FadeObstacle _currentObstacle;

    private void LateUpdate()
    {
        if (!cameraTransform) return;

        Vector3 origin = cameraTransform.position;
        Vector3 target = transform.position;
        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        // 1. Raycast logic
        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, obstructionMask))
        {
            // Visualize the hit in the Scene View
            if (showDebugRay) Debug.DrawRay(origin, dir.normalized * hit.distance, rayColorHit);

            FadeObstacle fade = hit.collider.GetComponent<FadeObstacle>();

            if (fade != null)
            {
                if (fade != _currentObstacle)
                {
                    Debug.Log($"<color=yellow>New Obstacle Detected:</color> {hit.collider.name}");
                    
                    if (_currentObstacle != null)
                        _currentObstacle.FadeIn();

                    fade.FadeOut();
                    _currentObstacle = fade;
                }
            }
            else
            {
                // Logic check: We hit something on the layer, but it lacks the script
                Debug.LogWarning($"Hit {hit.collider.name} on Obstruction Layer, but it is missing the FadeObstacle component!");
            }
        }
        else
        {
            // Visualize the clear path
            if (showDebugRay) Debug.DrawRay(origin, dir.normalized * dist, rayColorNoHit);

            if (_currentObstacle != null)
            {
                Debug.Log("<color=cyan>Path Clear:</color> Fading previous obstacle back in.");
                _currentObstacle.FadeIn();
                _currentObstacle = null;
            }
        }
    }
}