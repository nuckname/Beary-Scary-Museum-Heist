using UnityEngine;

public class DropOffZoneListener : MonoBehaviour
{
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            RoundStateManager.Instance.NotifyTriggerExit(other);
        }
       
    }
    
    private void OnTriggerEnter(Collider other)
    {
        RoundStateManager.Instance.NotifyTriggerEnter(other);
    }
}