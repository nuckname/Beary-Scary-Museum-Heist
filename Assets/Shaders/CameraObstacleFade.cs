using UnityEngine;

// https://www.youtube.com/shorts/D4xMei0nYW0
// this is not mine https://www.patreon.com/posts/how-to-make-144078368
public class CameraObstacleFade : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public LayerMask obstructionMask;
    public LayerMask forceHideObjects; 

    [Header("Debug Settings")]
    public bool showDebugRay = true;
    public Color rayColorHit = Color.red;
    public Color rayColorNoHit = Color.green;

    private FadeObstacles _currentObstacle;

    private void LateUpdate()
    {
        if (!cameraTransform) return;

        Vector3 origin = cameraTransform.position;
        Vector3 target = transform.position;
        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        LayerMask combinedMask = obstructionMask | forceHideObjects;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, combinedMask))
        {
            if (showDebugRay) Debug.DrawRay(origin, dir.normalized * hit.distance, rayColorHit);

            FadeObstacles fade = hit.collider.GetComponent<FadeObstacles>();

            if (fade != null)
            {
                // If we hit a new obstacle
                if (fade != _currentObstacle)
                {
                    // Tell the old obstacle the raycast left
                    if (_currentObstacle != null)
                        _currentObstacle.SetRaycastFade(false);

                    // Tell the new obstacle the raycast hit
                    fade.SetRaycastFade(true);
                    _currentObstacle = fade;
                }
            }
        }
        else
        {
            if (showDebugRay) Debug.DrawRay(origin, dir.normalized * dist, rayColorNoHit);

            // If the raycast is clear, tell the current obstacle the raycast left
            if (_currentObstacle != null)
            {
                _currentObstacle.SetRaycastFade(false);
                _currentObstacle = null;
            }
        }
    }
}