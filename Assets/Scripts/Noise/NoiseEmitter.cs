using UnityEngine;

public class NoiseEmitter : MonoBehaviour
{
    public LayerMask obstacleLayer; 
    public LayerMask enemyLayer;    

    [Header("2D Visual Circle")]
    public bool showVisibleRadius = true;
    public float visualDuration = 1.5f;
    public Material radiusMaterial;

    private float editorGizmoRadius = 4f;
    private Collider myCollider;

    private void Awake()
    {
        myCollider = GetComponent<Collider>();
    }

    /// <summary>
    /// Call this from any script to make a noise that AI can hear.
    /// The emitter automatically calculates its position at the bottom of the object.
    /// </summary>
    public void EmitNoise(float noiseRadius, NoiseType noiseType)
    {
        editorGizmoRadius = noiseRadius;

        // Spawn at the bottom of the object's collider
        Vector3 emissionPoint = transform.position;
        if (myCollider != null)
        {
            emissionPoint = myCollider.bounds.center;
            emissionPoint.y -= myCollider.bounds.extents.y;
        }

        if (showVisibleRadius)
        {
            ShowRadiusInGame(noiseRadius, emissionPoint);
        }

        BroadcastToAI(noiseRadius, emissionPoint, noiseType);
    }

    private void BroadcastToAI(float baseRange, Vector3 originPosition, NoiseType noiseType)
    {
        Collider[] entitiesInHearingRange = Physics.OverlapSphere(originPosition, baseRange, enemyLayer);

        foreach (Collider entity in entitiesInHearingRange)
        {
            if (entity.GetComponentInChildren<ISoundListener>() is ISoundListener listener)
            {
                float finalRange = baseRange;
                
                // Adjust for specific guard hearing sensitivity
                if(entity.GetComponentInChildren<GuardHearing>() is GuardHearing ear)
                {
                    finalRange *= ear.hearingSensitivity;
                }

                Vector3 directionToEntity = entity.transform.position - originPosition;
                float distanceToEntity = directionToEntity.magnitude;

                if (distanceToEntity <= finalRange)
                {
                    // Line of sight / acoustic occlusion check
                    if (!Physics.Raycast(originPosition, directionToEntity.normalized, out RaycastHit hit, distanceToEntity, obstacleLayer))
                    {
                        Debug.DrawLine(originPosition, entity.transform.position, Color.green, 2f);
                        listener.OnSoundHeard(originPosition, transform, noiseType);
                    }
                    else
                    {
                        Debug.DrawLine(originPosition, hit.point, Color.red, 2f);
                    }
                }
                else
                {
                    Debug.DrawLine(originPosition, entity.transform.position, Color.yellow, 2f);
                }
            }
        }
    }

    private void ShowRadiusInGame(float radius, Vector3 hitPosition)
    {
        GameObject flatCircle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        Destroy(flatCircle.GetComponent<Collider>());

        // + 0.02f so it's just above the ground to prevent Z-fighting/flickering
        flatCircle.transform.position = hitPosition + new Vector3(0, 0.15f, 0);

        float diameter = radius * 2f;
        flatCircle.transform.localScale = new Vector3(diameter, 0.01f, diameter);

        if (radiusMaterial != null)
        {
            flatCircle.GetComponent<Renderer>().material = radiusMaterial;
        }

        Destroy(flatCircle, visualDuration);
    }

    private void OnDrawGizmosSelected()
    {
        if (editorGizmoRadius <= 0f) return;

        Gizmos.color = Color.cyan;
        int segments = 36;
        float angle = 0f;
        Vector3 lastPoint = transform.position + new Vector3(editorGizmoRadius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            angle += (360f / segments) * Mathf.Deg2Rad;
            Vector3 nextPoint = transform.position + new Vector3(Mathf.Cos(angle) * editorGizmoRadius, 0, Mathf.Sin(angle) * editorGizmoRadius);
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }
}