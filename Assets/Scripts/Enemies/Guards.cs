using System;
using System.Collections;
using UnityEngine;

//https://www.youtube.com/watch?v=jUdx_Nj4Xk0
public class Guards : MonoBehaviour
{
    public Transform pathHolder;

    [SerializeField] private float walkSpeed = 2f;
    
    [SerializeField] private bool doesGuardWaitAtEachWaypoint = true;
    [SerializeField] private bool doesGuardTurnAroundAtEachWaypoint = true;
    [SerializeField] private float waitTime = 2f;
    
    [SerializeField] private float turnSpeed = 90f;

    private void Start()
    {
        Vector3[] waypoints = new Vector3[this.pathHolder.childCount];
        
        for (int i = 0; i < this.pathHolder.childCount; i++)
        {
            waypoints[i] = this.pathHolder.GetChild(i).position;    
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }
        
        StartCoroutine(FollowPath(waypoints));
    }

    private IEnumerator TurnToFace(Vector3 lookTarget)
    {
        Vector3 directionToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = Mathf.Atan2(directionToLookTarget.x, directionToLookTarget.z) * Mathf.Rad2Deg;

        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }
    
    private IEnumerator FollowPath(Vector3[] waypoints)
    {
        transform.position = waypoints[0];
        int targetWaypointIndex = 1;
        Vector3 targetWaypoint = waypoints[targetWaypointIndex];

        transform.LookAt(targetWaypoint);
        
        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, walkSpeed * Time.deltaTime);
            
            if (transform.position == targetWaypoint)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetWaypoint = waypoints[targetWaypointIndex];
                yield return new WaitForSeconds(waitTime);
                yield return StartCoroutine(TurnToFace(targetWaypoint));
            }
            
            yield return null;
        }
    }
    
    void OnDrawGizmos()
    {
        if (pathHolder != null)
        {
            Vector3 startPosition = pathHolder.GetChild(0).position;
            Vector3 previousPosition = startPosition;
            foreach (Transform child in pathHolder)
            {
                Gizmos.DrawLine(previousPosition, child.position);
                previousPosition = child.position;
                
                Gizmos.DrawSphere(child.position, 0.5f);
            }
            
            Gizmos.DrawLine(previousPosition, startPosition);
        }
    }
}
