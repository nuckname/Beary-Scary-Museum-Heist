using System;
using UnityEngine;

public class DoorCollision : MonoBehaviour
{
    //use transforms later
    
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CharacterController characterController;
    
    [SerializeField] private Vector3 playerTpLocation;
    [SerializeField] private Vector3 cameraTpLocation;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            //move player
            characterController.enabled = false;
            characterController.transform.position = playerTpLocation;
            characterController.enabled = true;
            
            //move camera
            playerCamera.transform.position = cameraTpLocation;
        }
    }
}