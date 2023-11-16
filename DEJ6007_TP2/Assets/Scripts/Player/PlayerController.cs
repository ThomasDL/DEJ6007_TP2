using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CharacterController controller;
    public Camera cam;

    public Gun[] gunList;
    public GameObject[] gunObjets;
    public Transform gunPoint;
    float delaySinceFiring = 0f;
    int currentGun;

    public Transform groundChecker;
    float groundCheckDistance = 0.58f;

    float speed = 12f;
    float gravity = -25f;
    float jumpForce = 15f;


    Vector3 verticalVelocity;

    void Update()
    {
        // Ground movement code
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // Groundcheck + jump code
        bool isGrounded = IsGrounded();
        if (isGrounded) verticalVelocity.y = 0f;
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            verticalVelocity.y = jumpForce;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);

        // Shooting code
        delaySinceFiring += Time.deltaTime;
        if (Input.GetButtonDown("Fire1") && delaySinceFiring > gunList[currentGun].gunCooldown)
        {
            delaySinceFiring = 0f;
            StartCoroutine(ShootBullet(gunList[currentGun].bulletAmount, gunList[currentGun].bulletSpeed, gunList[currentGun].bulletDelay, gunList[currentGun].bulletPrecision));
        }

        // Gun switching code
        int lastGun = currentGun;
        if (Input.GetButtonDown("Gun0")) currentGun = 0;
        if (Input.GetButtonDown("Gun1")) currentGun = 1;
        if (Input.GetButtonDown("Gun2")) currentGun = 2;
        if (Input.GetButtonDown("Gun3")) currentGun = 3;
        if (lastGun != currentGun) SwitchGun(lastGun);
    }
    void SwitchGun(int lastGun)
    {
        gunObjets[lastGun].SetActive(false);
        gunObjets[currentGun].SetActive(true);
    }
    bool IsGrounded()
    {
        return Physics.CheckSphere(groundChecker.position, groundCheckDistance, LayerMask.GetMask("Ground"));
    }
    IEnumerator ShootBullet(int amount, float speed, float delay, float precision)
    {
        for(int i = 0; i < amount; i++)
        {
            Rigidbody bulletRigidBody = Instantiate(gunList[currentGun].bulletPrefab, gunPoint.position, cam.transform.rotation).GetComponent<Rigidbody>();
            bulletRigidBody.AddForce(bulletRigidBody.transform.forward * speed + Random.insideUnitSphere * precision, ForceMode.Impulse);
            yield return new WaitForSeconds(delay);
        }
    }
}
[System.Serializable]
public class Gun
{
    public string gunName;
    public int bulletAmount;
    // 20 = slow, 200 = fast
    public float bulletSpeed;
    public float bulletDelay;
    public float gunCooldown;
    // 0 = super precise, 20f = very imprecise
    public float bulletPrecision;
    public GameObject bulletPrefab;
}
