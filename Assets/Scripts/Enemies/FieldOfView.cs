using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//this is from https://github.com/SebLague/Field-of-View/blob/master/Episode%2002/Scripts/FieldOfView.cs
public class FieldOfView : MonoBehaviour {

    public float viewRadius;
    [Range(0,360)]
    public float viewAngle;
    public float yDetectionRadius = 3f;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    public float meshResolution;
    public int edgeResolveIterations;
    public float edgeDstThreshold;

    public float maskCutawayDst = .1f;

    private bool wasSeeingPlayer = false;
    
    private bool currentlySeeingPlayer;
    private EnemyStateManager enemyStateManager;
    
    public MeshFilter viewMeshFilter;
    public LineRenderer edgeLineRenderer;
    public Material edgeLineMaterial;
    public Color edgeLineColor = Color.red;
    public float edgeLineWidth = 0.05f; 
    Mesh viewMesh;
    
    private NoiseEmitter noiseEmitter;
    private bool hasShoutedAtPlayer = false;
    
    bool seesPlayerThisFrame = false;
    Transform spottedPlayer = null;
    
    public event Action<Transform> OnPlayerSpotted;
    public event Action<Vector3> OnPlayerLost;
    
    private Vector3 lastKnownPlayerPosition;
    private float originalViewRadius;

    private float originalViewAngle;

    [SerializeField] private GameObject flashLightSource;
    
    private void Awake()
    {
       noiseEmitter = GetComponentInParent<NoiseEmitter>();
       enemyStateManager = GetComponentInParent<EnemyStateManager>();
       
       originalViewRadius = viewRadius;
       originalViewAngle = viewAngle;
    }

    void Start() {

       if (viewMeshFilter != null)
       {
          viewMesh = new Mesh ();
          viewMesh.name = "View Mesh";
          viewMeshFilter.mesh = viewMesh;
       }

       // Setup the Line Renderer's material, color, and width at start
       if (edgeLineRenderer != null) {
          if (edgeLineMaterial != null) {
             edgeLineRenderer.material = edgeLineMaterial;
          }
          edgeLineRenderer.startColor = edgeLineColor;
          edgeLineRenderer.endColor = edgeLineColor;
          
          // Apply the line thickness
          edgeLineRenderer.startWidth = edgeLineWidth;
          edgeLineRenderer.endWidth = edgeLineWidth;
       }

       StartCoroutine ("FindTargetsWithDelay", .2f);
    }


    IEnumerator FindTargetsWithDelay(float delay) {
       while (true) {
          yield return new WaitForSeconds (delay);
          FindVisibleTargets ();
       }
    }

    void LateUpdate() {
       if (viewMeshFilter != null)
       {
            DrawFieldOfView ();
       }
    }

    void FindVisibleTargets() {
       visibleTargets.Clear ();
       
       // Reset these locally every single time we check vision
       bool seesPlayerThisFrame = false;
       Transform spottedPlayer = null;
       
       Vector3 topPoint = transform.position + (Vector3.up * yDetectionRadius);
       Vector3 bottomPoint = transform.position - (Vector3.up * yDetectionRadius);
       Collider[] targetsInViewRadius = Physics.OverlapCapsule (bottomPoint, topPoint, viewRadius, targetMask);

       for (int i = 0; i < targetsInViewRadius.Length; i++) {
          Transform target = targetsInViewRadius [i].transform;
          
          Vector3 flattenedTargetPos = new Vector3(target.position.x, transform.position.y, target.position.z);
          Vector3 flattenedDirToTarget = (flattenedTargetPos - transform.position).normalized;
          
          if (Vector3.Angle (transform.forward, flattenedDirToTarget) < viewAngle / 2) {
             
             float dstToTarget = Vector3.Distance (transform.position, target.position);
             Vector3 trueDirToTarget = (target.position - transform.position).normalized;
             
             if (!Physics.Raycast (transform.position, trueDirToTarget, dstToTarget, obstacleMask)) {
                visibleTargets.Add (target);
                
                if (target.CompareTag("Player"))
                {
                   // Seen logic
                   seesPlayerThisFrame = true;
                   spottedPlayer = target;
                   lastKnownPlayerPosition = target.position;
                }
             }
          }
       }
       
       // Handle Event Broadcasting
       if (seesPlayerThisFrame) 
       {
          // For score board tracking
          // Only increment if we weren't seeing the player in the previous check
          // Stops getting called too many times
          // ? 
          if (!wasSeeingPlayer) 
          {
             RoundStateManager.AmountOfTimesPlayerSpottedByGuards++;
          }
          
          // called in EnemyStateManager.cs
          OnPlayerSpotted?.Invoke(spottedPlayer);
       }
       else if (wasSeeingPlayer && !seesPlayerThisFrame) {
          Vector3 dirToLastPos = (lastKnownPlayerPosition - transform.position).normalized;
          float angle = Vector3.SignedAngle(transform.forward, dirToLastPos, Vector3.up);

          if (angle > 0) {
             // Player left to the RIGHT
             enemyStateManager.playerLeftTheGuardsFovOnRightSide = true;
          } else {
             // Player left to the LEFT
             enemyStateManager.playerLeftTheGuardsFovOnRightSide = false;
          }

          OnPlayerLost?.Invoke(lastKnownPlayerPosition);
       }

       // Update our tracking variable for the next delay cycle
       wasSeeingPlayer = seesPlayerThisFrame;
    }

    void DrawFieldOfView() {
       int stepCount = Mathf.RoundToInt(viewAngle * meshResolution);
       float stepAngleSize = viewAngle / stepCount;
       List<Vector3> viewPoints = new List<Vector3> ();
       ViewCastInfo oldViewCast = new ViewCastInfo ();
       for (int i = 0; i <= stepCount; i++) {
          float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
          ViewCastInfo newViewCast = ViewCast (angle);

          if (i > 0) {
             bool edgeDstThresholdExceeded = Mathf.Abs (oldViewCast.dst - newViewCast.dst) > edgeDstThreshold;
             if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDstThresholdExceeded)) {
                EdgeInfo edge = FindEdge (oldViewCast, newViewCast);
                if (edge.pointA != Vector3.zero) {
                   viewPoints.Add (edge.pointA);
                }
                if (edge.pointB != Vector3.zero) {
                   viewPoints.Add (edge.pointB);
                }
             }

          }

          viewPoints.Add (newViewCast.point);
          oldViewCast = newViewCast;
       }

       int vertexCount = viewPoints.Count + 1;
       Vector3[] vertices = new Vector3[vertexCount];
       int[] triangles = new int[(vertexCount-2) * 3];

       vertices [0] = Vector3.zero;
       for (int i = 0; i < vertexCount - 1; i++) {
          vertices [i + 1] = transform.InverseTransformPoint(viewPoints [i]);

          if (i < vertexCount - 2) {
             triangles [i * 3] = 0;
             triangles [i * 3 + 1] = i + 1;
             triangles [i * 3 + 2] = i + 2;
          }
       }

       viewMesh.Clear ();

       viewMesh.vertices = vertices;
       viewMesh.triangles = triangles;
       viewMesh.RecalculateNormals ();
       
       if (edgeLineRenderer != null) {
          edgeLineRenderer.positionCount = viewPoints.Count + 2;
          edgeLineRenderer.SetPosition(0, transform.position);
          for (int i = 0; i < viewPoints.Count; i++) {
             edgeLineRenderer.SetPosition(i + 1, viewPoints[i]);
          }
          edgeLineRenderer.SetPosition(viewPoints.Count + 1, transform.position);
       }
    }

    public void DisableVision()
    {
       if (flashLightSource.gameObject.activeSelf)
       {
          flashLightSource.gameObject.SetActive(false);
       }
       
       viewRadius = 0f;
       visibleTargets.Clear(); 
       currentlySeeingPlayer = false;
       wasSeeingPlayer = false;
    }

    public void RestoreVision()
    {
       if (flashLightSource.gameObject.activeSelf)
       {
          flashLightSource.gameObject.SetActive(false);
       }
       
       viewRadius = originalViewRadius;
       viewAngle = originalViewAngle;
    }
    
    public void RestoreFOVRadius()
    {
       viewRadius = originalViewRadius;
    }
    
    /// <summary>
    /// E.g. A 50% reduction (0.50f) of a 10m radius makes it 5m
    /// 
    /// </summary>
    /// <param name="percentage"></param>
    public void ReduceFOVRadius(float percentage)
    {
       viewRadius = originalViewRadius * (1f - percentage);
    }

    EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast) {
       float minAngle = minViewCast.angle;
       float maxAngle = maxViewCast.angle;
       Vector3 minPoint = Vector3.zero;
       Vector3 maxPoint = Vector3.zero;

       for (int i = 0; i < edgeResolveIterations; i++) {
          float angle = (minAngle + maxAngle) / 2;
          ViewCastInfo newViewCast = ViewCast (angle);

          bool edgeDstThresholdExceeded = Mathf.Abs (minViewCast.dst - newViewCast.dst) > edgeDstThreshold;
          if (newViewCast.hit == minViewCast.hit && !edgeDstThresholdExceeded) {
             minAngle = angle;
             minPoint = newViewCast.point;
          } else {
             maxAngle = angle;
             maxPoint = newViewCast.point;
          }
       }

       return new EdgeInfo (minPoint, maxPoint);
    }


    ViewCastInfo ViewCast(float globalAngle) {
       Vector3 dir = DirFromAngle (globalAngle, true);
       RaycastHit hit;

       if (Physics.Raycast (transform.position, dir, out hit, viewRadius, obstacleMask)) {
          return new ViewCastInfo (true, hit.point, hit.distance, globalAngle);
       } else {
          return new ViewCastInfo (false, transform.position + dir * viewRadius, viewRadius, globalAngle);
       }
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal) {
       if (!angleIsGlobal) {
          angleInDegrees += transform.eulerAngles.y;
       }
       return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),0,Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo {
       public bool hit;
       public Vector3 point;
       public float dst;
       public float angle;

       public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle) {
          hit = _hit;
          point = _point;
          dst = _dst;
          angle = _angle;
       }
    }

    public struct EdgeInfo {
       public Vector3 pointA;
       public Vector3 pointB;

       public EdgeInfo(Vector3 _pointA, Vector3 _pointB) {
          pointA = _pointA;
          pointB = _pointB;
       }
    }

}