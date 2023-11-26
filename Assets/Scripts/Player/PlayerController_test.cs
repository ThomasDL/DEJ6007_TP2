using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_test : MonoBehaviour
{
    #region Movement

    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform groundChecker;
    [SerializeField] private GameObject translocatorScreen;

    private float groundCheckDistance = 0.58f;

    private float speed = 12f;
    private float gravity = -25f;
    private float jumpForce = 15f;

    Vector3 verticalVelocity;

    private bool isGrounded;

    #endregion


    #region Weapons

    [Header("Weapons settings")]
    public Gun_test[] gunList;
    public GameObject[] gunObjets;
    public Transform gunPoint;
    float delaySinceFiring = 0f;

    [HideInInspector] public int currentGun;
    private int lastGun;

    private bool canShoot;

    public bool CanShoot
    {
        get { return canShoot; }
    }

    #endregion

    #region ClippingPrevention

    [Header("Weapon clipping prevention")]
    [SerializeField] private GameObject weapon;
    [SerializeField] private GameObject clipProjector;
    [SerializeField] private float checkDistance;
    [Tooltip("Set the rotation to prevent clipping. Default is (0, -90, 0)")]
    [SerializeField] private Vector3 newDirection = new Vector3(0, -90, 0);

    // Threshold compared to weapon rotation when near an obstacle
    private float shootDisableThreshold = 0.15f;

    private float lerpPosition;
    RaycastHit hit;

    private void PreventClip()
    {
        if(Physics.Raycast(clipProjector.transform.position, clipProjector.transform.forward, out hit, checkDistance))
        {
            //Debug.Log("lerpPosition = " + lerpPosition);
            //Get percentage from 0 to max distance
            lerpPosition = 1 - (hit.distance / checkDistance);
        }
        else
        {
            //if we are not hitting anything, set to 0
            lerpPosition = 0;
        }

        lerpPosition = Mathf.Clamp01(lerpPosition);
        weapon.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(newDirection), lerpPosition);

        // Disable shooting if the weapon is rotated significantly
        canShoot = lerpPosition < shootDisableThreshold;
    }

    #endregion

    private void Start()
    {
        isGrounded = IsGrounded();
        canShoot = true;
    }

    void Update()
    {
        // Ground movement code
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // Groundcheck + jump code
        isGrounded = IsGrounded();
        if (isGrounded) verticalVelocity.y = 0f;
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            verticalVelocity.y = jumpForce;
        }

        verticalVelocity.y += gravity * Time.deltaTime;
        controller.Move(verticalVelocity * Time.deltaTime);

        PreventClip();

        // Shooting code
        delaySinceFiring += Time.deltaTime;
        if (Input.GetButtonDown("Fire1") && canShoot && delaySinceFiring > gunList[currentGun].gunCooldown)
        {
            delaySinceFiring = 0f;
            StartCoroutine(ShootBullet(gunList[currentGun].bulletAmount, gunList[currentGun].bulletSpeed, gunList[currentGun].bulletDelay, gunList[currentGun].bulletPrecision));
        }

        // Gun switching code
        lastGun = currentGun;
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
        for (int i = 0; i < amount; i++)
        {
            var bulletObject = Instantiate(gunList[currentGun].bulletPrefab, gunPoint.position, cam.transform.rotation);

            //Special behavior for Teleporter Gun
            if (currentGun == 2)
            {
                Debug.Log("Shooting TP Bullet");
                bulletObject.GetComponent<TeleporterBulletBehavior_test>().GetTranslocatorScreenReference(translocatorScreen);
                bulletObject.GetComponent<TeleporterBulletBehavior_test>().Shoot_TP_Bullet();
            }
            else
            {
                bulletObject.GetComponent<Rigidbody>().AddForce(bulletObject.transform.forward * speed + Random.insideUnitSphere * precision, ForceMode.Impulse);
            }

            yield return new WaitForSeconds(delay);
        }
    }
}

[System.Serializable]
public class Gun_test
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
