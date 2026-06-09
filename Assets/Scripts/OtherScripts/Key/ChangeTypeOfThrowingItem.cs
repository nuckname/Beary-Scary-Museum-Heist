using System;
using UnityEngine;

public class ChangeTypeOfThrowingItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("CanPickUp"))
        {
            if(other.gameObject.TryGetComponent(out IsKey key))
            {
                if(key != null)
                {
                    key.GetComponent<CanPickUpItem>().typeOfItem = ItemType.KeySmallThrow;
                }
            }
        }

        if (other.gameObject.CompareTag("Player"))
        {
            if(other.gameObject.GetComponentInChildren<IsKey>())
            {
                IsKey key = other.gameObject.GetComponentInChildren<IsKey>();
                
                if(key != null)
                {
                    key.GetComponent<CanPickUpItem>().typeOfItem = ItemType.KeySmallThrow;
                }
            }
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("CanPickUp"))
        {
            if(other.gameObject.TryGetComponent(out IsKey key))
            {
                if(key != null)
                {
                    key.GetComponent<CanPickUpItem>().typeOfItem = ItemType.KeyBigThrow;
                }
            }
        }
    }
}
