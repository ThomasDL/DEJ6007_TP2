using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;
    public Camera cam;

    public GameObject bulletPrefab;
    public Transform gunPoint;
    float delaySinceFiring = 0f;

    public Transform groundChecker;
    float groundDistance = 0.58f;

    float speed = 12f;
    float gravity = -25f;
    float jumpForce = 15f;

    List<Gun> gunList = new List<Gun>();

    Vector3 velocity;
    private void Start()
    {
        gunList.Add(new Gun("Machine Gun", 5, 90f, 0.001f, 15f, 0.2f));
    }

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

        delaySinceFiring += Time.deltaTime;
        if (Input.GetMouseButtonDown(0) && delaySinceFiring > gunList[0].gunCooldown)
        {
            delaySinceFiring = 0f;
            StartCoroutine(ShootBullet(gunList[0].bulletAmount, gunList[0].bulletSpeed, gunList[0].bulletDelay, gunList[0].bulletPrecision));
        }
    }
    bool IsGrounded()
    {
        return Physics.CheckSphere(groundChecker.position, groundDistance, LayerMask.GetMask("Ground"));
    }
    IEnumerator ShootBullet(int amount, float speed, float delay, float precision)
    {
        for(int i = 0; i < amount; i++)
        {
            Rigidbody bulletRigidBody = Instantiate(bulletPrefab, gunPoint.position, cam.transform.rotation).GetComponent<Rigidbody>();
            bulletRigidBody.rotation = Quaternion.Euler(cam.transform.rotation.eulerAngles.x + 90, cam.transform.rotation.eulerAngles.y, cam.transform.rotation.eulerAngles.z);
            bulletRigidBody.AddForce(bulletRigidBody.transform.forward * speed + Random.insideUnitSphere * precision, ForceMode.Impulse);
            yield return new WaitForSeconds(delay);
        }
    }
}
public class Gun
{
    public string gunName;
    public int bulletAmount;
    public float bulletSpeed;
    public float bulletDelay;
    public float gunCooldown;

    // 0 = precise, 15f = very imprecise
    [Range(0f, 15f)]
    public float bulletPrecision;

    public Gun(string name, int amount, float speed, float delay, float precision, float cooldown)
    {
        gunName = name;
        bulletAmount = amount;
        bulletSpeed = speed;
        bulletDelay = delay;
        bulletPrecision = precision;
        gunCooldown = cooldown;
    }
}
