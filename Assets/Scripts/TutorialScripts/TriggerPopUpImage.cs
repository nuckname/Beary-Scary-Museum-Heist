using System;
using UnityEngine;

public class TriggerPopUpImage : MonoBehaviour
{
    [SerializeField] private GameObject popUpImage;

    private void Start()
    {
        popUpImage.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            popUpImage.SetActive(true);
        }
    }
}
