using UnityEngine;

public class PlayerStealthController : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 9f;
    
    public CharacterController controller;
    
    private Vector3 moveDirection;
    private float currentSpeed;

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = sprintSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector3(horizontal, 0f, vertical).normalized;

        controller.Move(moveDirection * currentSpeed * Time.deltaTime);
        
        if (moveDirection.magnitude >= 0.1f)
        {

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 15f * Time.deltaTime);
        }
    }
}