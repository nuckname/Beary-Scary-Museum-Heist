using System;
using UnityEngine;

public class PlayDogAwake : MonoBehaviour
{
    [SerializeField] private TutorialFollowPath tutorialFollowPath;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            tutorialFollowPath.forceWakeUp = true; 
        }
    }
}