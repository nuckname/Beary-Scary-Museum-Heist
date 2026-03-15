using System;
using UnityEngine;

public class BoxCollision : MonoBehaviour
{
    public float boxWeight;
    public float boxWorthAmount;
    public bool hasEnteredDropOffZone = false;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("DropOffZone"))
        {
            if (!hasEnteredDropOffZone)
            {
                other.GetComponent<UpdateQuota>().totalQuota += boxWorthAmount;
            }
            
            hasEnteredDropOffZone = true;
        }
    }
}
