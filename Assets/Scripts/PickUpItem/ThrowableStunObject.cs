using UnityEngine;

public class ThrowableStunObject : MonoBehaviour, IPickable
{
    [Header("Stun Settings")]
    public bool canOnlyStunWhenAirBorne = true; 
    public bool objectCanStunGuard = true;
    [SerializeField] private LayerMask whatIsGround;
    
    public void OnPickedUp()
    {
        objectCanStunGuard = true;
    }

    public void OnReleased()
    {
        // Leave empty. The main PickupItem script handles the physics of dropping.
    }

    public bool IsOnGround()
    {
        if (TryGetComponent(out PickUpItem pickup))
        {
            return pickup.IsOnGround();
        }
        return false; 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) return;

        if ((whatIsGround.value & (1 << collision.gameObject.layer)) != 0)
        {
            objectCanStunGuard = false;
        }

        // Apply Stun
        if (collision.gameObject.CompareTag("Enemy") && objectCanStunGuard)
        {
            if (collision.gameObject.TryGetComponent(out EnemyStateManager enemy))
            {
                enemy.SwitchState(enemy.EnemyStunnedState);
            }
        }
    }
}