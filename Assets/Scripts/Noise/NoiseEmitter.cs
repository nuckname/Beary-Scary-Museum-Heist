using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Call this from any script to make a noise that AI can hear.
/// The emitter automatically calculates its position at the bottom of the object.
/// </summary>
public class NoiseEmitter : MonoBehaviour
{
    public LayerMask obstacleLayer; 
    public LayerMask enemyLayer;    

    [Header("2D Visual Circle")]
    public bool showVisibleRadius = true;
    public float visualDuration = 1.5f;
    public Material radiusMaterial;
    [Tooltip("Height offset above the surface for the inner circle mesh.")]
    public float meshHeightOffset = 1f;
    
    [Header("Outline Settings")]
    [Tooltip("Check to draw an outline around the noise radius.")]
    public bool showOutline = true;
    [Tooltip("The material used for the outline (Use an Unlit or Sprite material for a clean look).")]
    public Material outlineMaterial;
    public float outlineWidth = 0.1f;
    [Tooltip("Height offset above the surface for the LineRenderer outline.")]
    public float outlineHeightOffset = 0.12f;

    [Header("Mesh Resolution")]
    [Tooltip("How many rays per degree. Higher means smoother edges but costs more performance.")]
    public float meshResolution = 1f;

    private float editorGizmoRadius = 4f;
    private Collider myCollider;

    private void Awake()
    {
        myCollider = GetComponent<Collider>();
    }

    public void EmitNoise(float noiseRadius, NoiseType noiseType)
    {
        editorGizmoRadius = noiseRadius;

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

private void BroadcastToAI(float noiseRadius, Vector3 originPosition, NoiseType noiseType)
    {
        // We need a huge physical net to catch guards who are standing far away 
        // but have massive hearing ranges. OverlapSphere only finds their physical bodies!
        float overlapNetRadius = noiseRadius + 50f; 
        
        Collider[] entitiesInArea = Physics.OverlapSphere(originPosition, overlapNetRadius, enemyLayer);

        foreach (Collider entity in entitiesInArea)
        {
            EnemyStateManager guard = entity.GetComponentInParent<EnemyStateManager>();
            
            if (guard != null && guard.hearingFOV != null)
            {
                float distanceToGuard = Vector3.Distance(originPosition, entity.transform.position);
                float guardHearingRadius = guard.hearingFOV.viewRadius;

                // Are they close enough to hear?
                if (distanceToGuard <= (noiseRadius + guardHearingRadius))
                {
                    // Is there a wall blocking the sound?
                    // We raise the raycast slightly off the ground (Vector3.up * 0.5f) 
                    // so it doesn't accidentally scrape the floor mesh and register as a false collision.
                    Vector3 raycastStart = originPosition + Vector3.up * 0.5f;
                    Vector3 raycastTarget = entity.transform.position + Vector3.up * 0.5f;

                    // Linecast returns true if it hits something. 
                    if (!Physics.Linecast(raycastStart, raycastTarget, obstacleLayer))
                    {
                        if (entity.GetComponentInChildren<ISoundListener>() is ISoundListener listener)
                        { 
                            // Called in EnemyStateManager
                            listener.OnSoundHeard(originPosition, transform, noiseType);
                        }
                    }
                }
            }
        }
    }

    // Ai + Me
    // https://gemini.google.com/share/1d2964621e3c
    private void ShowRadiusInGame(float radius, Vector3 hitPosition)
    {
        // Create a new empty GameObject
        GameObject visualMeshObj = new GameObject("NoiseVisual");
        
        // Use the new specific offset for the Mesh
        visualMeshObj.transform.position = hitPosition + new Vector3(0, meshHeightOffset, 0);

        MeshFilter meshFilter = visualMeshObj.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = visualMeshObj.AddComponent<MeshRenderer>();
        
        if (radiusMaterial != null)
        {
            meshRenderer.material = radiusMaterial;
        }

        // Calculate vertices
        int stepCount = Mathf.RoundToInt(360f * meshResolution);
        float stepAngleSize = 360f / stepCount;
        List<Vector3> viewPoints = new List<Vector3>();
        List<Vector3> outlinePoints = new List<Vector3>();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = stepAngleSize * i;
            Vector3 dir = DirFromAngle(angle);

            Vector3 targetPoint;
            if (Physics.Raycast(hitPosition, dir, out RaycastHit hit, radius, obstacleLayer))
            {
                targetPoint = hit.point;
            }
            else
            {
                targetPoint = hitPosition + dir * radius;
            }

            viewPoints.Add(targetPoint);
            
            // Use the specific offset for the LineRenderer Outline
            outlinePoints.Add(targetPoint + new Vector3(0, outlineHeightOffset, 0)); 
        }

        // Build the Mesh
        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero; // The center of the mesh is local Vector3.zero
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = visualMeshObj.transform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;

        // outline render 
        if (showOutline && outlineMaterial != null)
        {
            LineRenderer lineRenderer = visualMeshObj.AddComponent<LineRenderer>();
            lineRenderer.material = outlineMaterial;
            lineRenderer.startWidth = outlineWidth;
            lineRenderer.endWidth = outlineWidth;
            lineRenderer.useWorldSpace = true; // Still using world space
            lineRenderer.loop = true;
            lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;

            lineRenderer.positionCount = outlinePoints.Count;
            lineRenderer.SetPositions(outlinePoints.ToArray());
        }

        Destroy(visualMeshObj, visualDuration);
    }

    private Vector3 DirFromAngle(float angleInDegrees)
    {
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    // AI
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