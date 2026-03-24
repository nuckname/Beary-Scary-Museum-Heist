using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//this is from https://github.com/SebLague/Field-of-View/blob/master/Episode%2002/Scripts/FieldOfView.cs
public class FieldOfView : MonoBehaviour {

	public float viewRadius;
	[Range(0,360)]
	public float viewAngle;

	public LayerMask targetMask;
	public LayerMask obstacleMask;

	[HideInInspector]
	public List<Transform> visibleTargets = new List<Transform>();

	private bool wasSeeingPlayer = false;
	
	public float meshResolution;
	public int edgeResolveIterations;
	public float edgeDstThreshold;

	private bool currentlySeeingPlayer;
	private EnemyStateManager enemyStateManager;
	
	public float maskCutawayDst = .1f;
	
	public MeshFilter viewMeshFilter;
	Mesh viewMesh;

	void Start() {
		viewMesh = new Mesh ();
		viewMesh.name = "View Mesh";
		viewMeshFilter.mesh = viewMesh;

		enemyStateManager = GetComponentInParent<EnemyStateManager>();
		
		StartCoroutine ("FindTargetsWithDelay", .2f);
	}

	IEnumerator FindTargetsWithDelay(float delay) {
		while (true) {
			yield return new WaitForSeconds (delay);
			FindVisibleTargets ();
		}
	}

	void LateUpdate() {
		DrawFieldOfView ();
	}

	// https://gemini.google.com/share/3e708dd40efa
void FindVisibleTargets() {
	
       visibleTargets.Clear ();
       
       // Assume we can't see the player until proven otherwise
       currentlySeeingPlayer = false; 

       Collider[] targetsInViewRadius = Physics.OverlapSphere (transform.position, viewRadius, targetMask);

       for (int i = 0; i < targetsInViewRadius.Length; i++) {
          Transform target = targetsInViewRadius [i].transform;
      
          // 1. Flatten the positions to the XZ plane for the Angle Check (to match the 2D mesh)
          Vector3 flatEnemyPos = new Vector3(transform.position.x, 0, transform.position.z);
          Vector3 flatTargetPos = new Vector3(target.position.x, 0, target.position.z);
          Vector3 dirToTargetFlat = (flatTargetPos - flatEnemyPos).normalized;
      
          // 2. Get the flat forward direction of the enemy (ignoring up/down tilt)
          Vector3 flatForward = DirFromAngle(0, false);

          // 3. Perform the angle check using the perfectly flat vectors
          if (Vector3.Angle (flatForward, dirToTargetFlat) < viewAngle / 2) {
         
             // 4. Calculate true 3D direction ONLY for the Raycast so low/high walls block vision
             Vector3 dirToTarget3D = (target.position - transform.position).normalized;
             float dstToTarget = Vector3.Distance (transform.position, target.position);
         
             // 5. Perform the line-of-sight check
             if (!Physics.Raycast (transform.position, dirToTarget3D, dstToTarget, obstacleMask)) {
	             visibleTargets.Add (target);
            
	             if (target.CompareTag("Player")) 
	             {
		             currentlySeeingPlayer = true;
		             enemyStateManager.StartChasing(target);
	             }
             }
          }
       }

       if (wasSeeingPlayer && !currentlySeeingPlayer) 
       {
           enemyStateManager.SwitchState(enemyStateManager.EnemyLostPlayerState);
       }

       // Update our tracking variable for the next delay cycle
       wasSeeingPlayer = currentlySeeingPlayer;
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
			vertices [i + 1] = transform.InverseTransformPoint(viewPoints [i]) + Vector3.forward * maskCutawayDst;

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
	
	// A temp floating text solution to show when the player is spotted by the guard, this is not the best way to do this but it works for now.
	// gets called too many times per hit
	void OnGUI() {
		GUI.color = Color.red;
		GUI.skin.label.fontSize = 20;
		GUI.skin.label.fontStyle = FontStyle.Bold;

		foreach (Transform target in visibleTargets) {
			if (target.CompareTag("Player")) {
				Vector3 worldPos = target.position + Vector3.up * 2.0f;
                
				Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

				if (screenPos.z > 0) {
					float guiX = screenPos.x;
					float guiY = Screen.height - screenPos.y;

					GUI.Label(new Rect(guiX - 50, guiY, 150, 30), "SPOTTED!");
				}
			}
		}
	}

}