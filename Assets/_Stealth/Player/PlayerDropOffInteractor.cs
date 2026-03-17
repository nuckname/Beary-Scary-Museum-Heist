using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDropOffInteractor : MonoBehaviour
{
    [SerializeField] private float dropOffCooldown = 10f;
    private float nextDropOffTime = 0f;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("DropOffZone"))
        {
            if (Time.time >= nextDropOffTime && Keyboard.current.eKey.wasPressedThisFrame)
            {
                RoundManager.Instance.NextRound();
                nextDropOffTime = Time.time + dropOffCooldown;
            }
        }
    }
}