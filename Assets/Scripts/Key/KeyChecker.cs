using System;
using UnityEngine;

public class KeyChecker : MonoBehaviour
{
    [SerializeField] private GameObject doorToOpen;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("CanPickUp"))
        {
            other.gameObject.TryGetComponent(out IKey key);
            if (key != null && !key.hasBeenUsed)
            {
                key.hasBeenUsed = true;
                doorToOpen.SetActive(false);
            }
        }
    }
}
