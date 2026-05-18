using System;
using Unity.VisualScripting;
using UnityEngine;

public class MakeItemThrowable : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.GetComponentInChildren<CanPickUpItem>().SetThrowableState(true);
        }
    }
}
