using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    float speed = 12f;
    float gravity = -20f;
    float jumpForce = 10f;

    public Vector3 velocity;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if (IsGrounded()) velocity.y = 0f;
        if (Input.GetButtonDown("Jump") && IsGrounded())
        {
            velocity.y = jumpForce;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    bool IsGrounded()
    {
        return Physics.CheckSphere(transform.position - new Vector3(0, controller.height * 0.5f - controller.radius*0.8f, 0), controller.radius*0.95f, LayerMask.GetMask("Ground"));
    }
}
