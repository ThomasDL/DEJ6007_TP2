using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Camera cam;

    public GameObject bulletPrefab;
    public Transform gunPoint;

    public Transform groundChecker;
    float groundDistance = 0.58f;

    float speed = 12f;
    float gravity = -25f;
    float jumpForce = 15f;

    Vector3 velocity;
 
    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        bool isGrounded = IsGrounded();
        if (isGrounded) velocity.y = 0f;
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = jumpForce;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        if (Input.GetMouseButtonDown(0))
        {
            ShootBullet();
        }
    }
    bool IsGrounded()
    {
        return Physics.CheckSphere(groundChecker.position, groundDistance, LayerMask.GetMask("Ground"));
    }
    void ShootBullet()
    {
        Debug.Log("Yeah");
        float bulletForce = 50f;
        Rigidbody bulletRigidBody = Instantiate(bulletPrefab, gunPoint.position, cam.transform.rotation).GetComponent<Rigidbody>();
        bulletRigidBody.rotation = Quaternion.Euler(cam.transform.rotation.eulerAngles.x + 90, cam.transform.rotation.eulerAngles.y, cam.transform.rotation.eulerAngles.z);
        bulletRigidBody.AddForce(bulletRigidBody.transform.forward * bulletForce, ForceMode.Impulse);
    }
}
