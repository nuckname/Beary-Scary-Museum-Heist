using System;
using UnityEngine;

public class EnemyStunCollision : MonoBehaviour
{
    private EnemyStateManager stateManager;

    private void Start()
    {
        stateManager = GetComponent<EnemyStateManager>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("CanPickUp"))
        {
            stateManager.SwitchState(stateManager.EnemyStunnedState);
        }
    }
}
