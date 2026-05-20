using UnityEngine;

public class MakeItemThrowable : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CanPickUpItem pickUpComponent = other.GetComponentInChildren<CanPickUpItem>();

            if (pickUpComponent != null)
            {
                pickUpComponent.SetThrowableState(true);
            }
            else
            {
                Debug.LogWarning("Error");
            }
        }
    }
}
