using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController_test : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Camera cam;
    [SerializeField] private Transform groundChecker;
    [SerializeField] private GameObject capsuleCharacter;

    #region Movement

    [Header("Horizontal movement")]
    [SerializeField] private float walkSpeed = 12f;
    [SerializeField] private float sprintSpeed = 24f;

    [Header("Vertical movement")]
    [SerializeField] private float gravity = -25f;
    [SerializeField] private float jumpForce = 15f;

    Vector3 verticalVelocity;
    [SerializeField] private float groundCheckDistance = 0.58f;
    private bool isGrounded;

    [Header("Movement options")]
    // Sprint and Jump controls
    [SerializeField] private bool canSprint = true; // Toggle sprint functionality
    [SerializeField] private bool canJump = true; // Toggle jump functionality
    [SerializeField] private bool canCrouch = true; // Toggle crouch functionality

    private KeyCode sprintKey = KeyCode.LeftShift;
    private KeyCode jumpKey = KeyCode.Space;


    // Properties to control movement + sprinting, jumping, and crouching
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && isGrounded;
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && isGrounded;

    [Header("Crouching parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 1.8f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    [SerializeField] private Vector3 standingCenter = Vector3.zero;
    private bool isCrouching = false;
    private bool duringCrouchAnimation = false;
    private KeyCode crouchKey = KeyCode.LeftControl; // Key for crouching

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

    //Easier reference spot for the translocator screen for now
    [SerializeField] private GameObject translocatorScreen;

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
        if (Physics.Raycast(clipProjector.transform.position, clipProjector.transform.forward, out hit, checkDistance))
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
        if (CanMove)
        {
            HandleMovementInput();
            if(canJump) HandleJump();
            if (canCrouch)
            {
                HandleCrouch();
            }
        }

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


    private void HandleMovementInput()
    {
        // Ground movement code
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float speed = IsSprinting ? sprintSpeed : walkSpeed;

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * speed * Time.deltaTime);

        // Groundcheck + jump code
        isGrounded = IsGrounded();
        if (isGrounded) verticalVelocity.y = 0f;
    }

    private void HandleJump()
    {
        if (ShouldJump)
        {
            verticalVelocity.y = jumpForce;
        }

        if(!isGrounded && !duringCrouchAnimation) verticalVelocity.y += gravity * Time.deltaTime;

        controller.Move(verticalVelocity * Time.deltaTime);
    }
    private void HandleCrouch()
    {
        if (ShouldCrouch)
        {
            StartCoroutine(CrouchStand());
        }
    }

    private IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(cam.transform.position, Vector3.up, 1f))
            yield break;

        duringCrouchAnimation = true;

        float timeElapsed = 0f;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = controller.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = controller.center;

        float currentCapsuleHeight = capsuleCharacter.transform.localScale.y;
        Vector3 currentCapsulePosition = capsuleCharacter.transform.localPosition;



        while (timeElapsed < timeToCrouch)
        {
            // Modify the capsuleCharacter's scale
            float newCapsuleHeight = Mathf.Lerp(currentCapsuleHeight, targetHeight / 2f, timeElapsed / timeToCrouch);
            capsuleCharacter.transform.localScale = new Vector3(capsuleCharacter.transform.localScale.x, newCapsuleHeight, capsuleCharacter.transform.localScale.z);

            // Gradually move the capsule position to 1 on the Y-axis
            float newYPosition = Mathf.Lerp(currentCapsulePosition.y, 1f, timeElapsed / timeToCrouch);
            capsuleCharacter.transform.localPosition = new Vector3(capsuleCharacter.transform.localPosition.x, newYPosition, capsuleCharacter.transform.localPosition.z);

            controller.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            controller.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure exact target values are set
        capsuleCharacter.transform.localScale = new Vector3(capsuleCharacter.transform.localScale.x, targetHeight / 2f, capsuleCharacter.transform.localScale.z);
        capsuleCharacter.transform.localPosition = new Vector3(capsuleCharacter.transform.localPosition.x, 1f, capsuleCharacter.transform.localPosition.z);

        controller.height = targetHeight;
        controller.center = targetCenter;

        isCrouching = !isCrouching;
        duringCrouchAnimation = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(groundChecker.position, new Vector3(groundChecker.position.x, groundChecker.position.y - groundCheckDistance, groundChecker.position.z));
    }

    void SwitchGun(int lastGun)
    {
        gunObjets[lastGun].SetActive(false);
        gunObjets[currentGun].SetActive(true);
    }

    bool IsGrounded()
    {
        Debug.DrawLine(groundChecker.position, new Vector3(groundChecker.position.x, groundChecker.position.y - groundCheckDistance, groundChecker.position.z), Color.red);
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
