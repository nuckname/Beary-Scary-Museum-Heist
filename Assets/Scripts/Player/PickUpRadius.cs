using System;
using UnityEngine;

// Turn on and off item images.
public class PickUpRadius : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("CanPickUp"))
        {
            ItemImage itemImage = other.gameObject.GetComponent<ItemImage>();
            
            
            if (itemImage != null && itemImage.allowImageToShow)
            {
                itemImage.spriteRenderer.enabled = true;
            }
            
            itemImage.allowImageToShow = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("CanPickUp"))
        {
            ItemImage itemImage = other.gameObject.GetComponent<ItemImage>();
            
            if (itemImage != null)
            {
                itemImage.spriteRenderer.enabled = false;
            }
        }
    }
}
