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
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float sprintSpeed = 14f;
    [SerializeField] private float slopeSpeed = 16f;
    private float crouchSpeed; // will be assigned in start as a % of walkspeed
    private Vector3 moveDirection;
    private float moveSpeed;
    private bool startedSprinting = false;

    [Header("Vertical movement")]
    [SerializeField] private float gravity = -30f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Coyote Time")]
    private float coyoteTimeDuration = 15f / 60f; // A coyote jump timeframe equivalent to 15 frames at 60 FPS
    private float timeSinceUngrounded = 0f;
    private bool justJumped = false;

    Vector3 verticalVelocity;
    [SerializeField] private float groundCheckDistance = 0.25f;
    private bool isGrounded;

    [Header("Modular movement options")]
    [SerializeField] private bool canSprint = true; // Enable sprint logic
    [SerializeField] private bool canJump = true; // Enable jump logic
    [SerializeField] private bool canCrouch = true; // Enable crouch logic
    [SerializeField] private bool canUseHeadBob = true; // Enable Head bob logic
    [SerializeField] private bool willSlideOnSlopes = true; // Enable slopes logic

    // Sprint and Jump controls
    private KeyCode sprintKey = KeyCode.LeftShift;
    private KeyCode jumpKey = KeyCode.Space;


    // Properties to control movement + sprinting, jumping, and crouching
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    //private bool ShouldJump => Input.GetKeyDown(jumpKey) && isGrounded;
    private bool ShouldJump => Input.GetKeyDown(jumpKey);
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

    [Header("HeadBob parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float defaultYPos = 0;
    private float timer;

    #endregion


    #region Weapons

    [Header("Weapons parameters")]
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

    [Tooltip("Layers to be ignored when preventing clipping")]
    [SerializeField] private LayerMask ignoredLayers;

    [Tooltip("Set the rotation to prevent clipping. Default is (0, -90, 0)")]
    [SerializeField] private Vector3 newDirection = new Vector3(0, -90, 0);

    // Threshold compared to weapon rotation when near an obstacle
    private float shootDisableThreshold = 0.15f;

    private float lerpPosition;
    RaycastHit hit;

    private void PreventClip()
    {
        // Define a layer mask that ignores the Player and Gun layers
        int layerMaskToIgnore = ~ignoredLayers; // Invert the mask to ignore these layers

        if (Physics.Raycast(clipProjector.transform.position, clipProjector.transform.forward, out hit, checkDistance, layerMaskToIgnore))
        {
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

    private void Awake()
    {
        defaultYPos = cam.transform.localPosition.y;
    }

    private void Start()
    {
        isGrounded = IsGrounded();
        canShoot = true;
        crouchSpeed = walkSpeed * 0.35f;
    }

    void Update()
    {
        // Groundcheck
        isGrounded = IsGrounded();

        if (CanMove)
        {
            HandleMovementInput();
            if (canJump) HandleJump();
            if (canCrouch) HandleCrouch();
            if (canUseHeadBob) HandleHeadBob();
        }

        PreventClip();

        HandleFiring();

        HandleGunSwitch();
    }

    private void HandleMovementInput()
    {
        // Ground movement code
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        startedSprinting = Input.GetKeyDown(sprintKey);

        moveDirection = transform.right * x + transform.forward * z;
        controller.Move(moveSpeed * Time.deltaTime * moveDirection);

        if (isGrounded)
        {
            moveSpeed = isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;
            verticalVelocity.y = 0f;
        }
    }

    private void HandleJump()
    {
        
        if (isGrounded)
        {
            //Reset consumed coyote time and justJumped flag
            timeSinceUngrounded = 0f;
            if (justJumped == true) justJumped = false;
            
            //Perform regular jump when grounded
            if (ShouldJump)
            {
                justJumped = true;
                verticalVelocity.y = jumpForce;
            }
        }
        else // Apply gravity and update coyote time and justJumped flag
        {
            verticalVelocity.y += gravity * Time.deltaTime;

            timeSinceUngrounded += Time.deltaTime;
            bool canCoyoteJump = (timeSinceUngrounded <= coyoteTimeDuration) && (justJumped == false);

            //Perform jump using coyote time
            if (ShouldJump && canCoyoteJump && !justJumped)
            {
                justJumped = true;
                Debug.Log("JUMP using COYOTE TIME");
                verticalVelocity.y = jumpForce;
            }
        }

        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        //Player can exit crouching either by pressing crouchKey again or when player starts running
        if ((!isCrouching && ShouldCrouch) || (isCrouching && (ShouldCrouch || startedSprinting)))
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
        float targetCapsuleYPositon = isCrouching ? 0f : 1f;
        float currentHeight = controller.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = controller.center;

        float currentCapsuleHeight = capsuleCharacter.transform.localScale.y;
        Vector3 currentCapsulePosition = capsuleCharacter.transform.localPosition;

        // Define a small lift amount when standing up
        float initialPlayerYPosition = transform.position.y;
        //float liftAmount = isCrouching ? 1.93f : 0f; // Lift player slightly when standing up
        float liftAmount = isCrouching ? standingHeight * 0.55f : 0f; // Lift player slightly when standing up

        //Scripted animation sequence for the crouching state
        while (timeElapsed < timeToCrouch)
        {
            //Adjusting the capsule to match the CharacterController collider new values
            float newCapsuleHeight = Mathf.Lerp(currentCapsuleHeight, targetHeight / 2f, timeElapsed / timeToCrouch);
            capsuleCharacter.transform.localScale = new Vector3(capsuleCharacter.transform.localScale.x, newCapsuleHeight, capsuleCharacter.transform.localScale.z);

            float newYPosition = Mathf.Lerp(currentCapsulePosition.y, targetCapsuleYPositon, timeElapsed / timeToCrouch);
            capsuleCharacter.transform.localPosition = new Vector3(capsuleCharacter.transform.localPosition.x, newYPosition, capsuleCharacter.transform.localPosition.z);

            //Gradually assigning new values for the CharacterController collider
            controller.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed / timeToCrouch);
            controller.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed / timeToCrouch);

            // Gradually lift the player when standing up
            if (isCrouching)
            {
                transform.position = new Vector3(transform.position.x, initialPlayerYPosition + liftAmount * (timeElapsed / timeToCrouch), transform.position.z);
            }

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        capsuleCharacter.transform.localScale = new Vector3(capsuleCharacter.transform.localScale.x, targetHeight / 2f, capsuleCharacter.transform.localScale.z);
        capsuleCharacter.transform.localPosition = new Vector3(capsuleCharacter.transform.localPosition.x, targetCapsuleYPositon, capsuleCharacter.transform.localPosition.z);

        controller.height = targetHeight;
        controller.center = targetCenter;
        transform.position = new Vector3(transform.position.x, initialPlayerYPosition + liftAmount, transform.position.z);

        isCrouching = !isCrouching;
        duringCrouchAnimation = false;
    }

    private void HandleHeadBob()
    {
        if (!isGrounded) return;

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            cam.transform.localPosition = new Vector3(
                cam.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                cam.transform.localPosition.z);
        }
    }



    private void HandleGunSwitch()
    {
        lastGun = currentGun;
        if (Input.GetButtonDown("Gun0")) currentGun = 0;
        if (Input.GetButtonDown("Gun1")) currentGun = 1;
        if (Input.GetButtonDown("Gun2")) currentGun = 2;
        if (Input.GetButtonDown("Gun3")) currentGun = 3;
        if (lastGun != currentGun) SwitchGun(lastGun);
    }

    private void HandleFiring()
    {
        // Shooting code
        delaySinceFiring += Time.deltaTime;
        if (Input.GetButtonDown("Fire1") && canShoot && delaySinceFiring > gunList[currentGun].gunCooldown)
        {
            delaySinceFiring = 0f;
            StartCoroutine(ShootBullet(gunList[currentGun].bulletAmount, gunList[currentGun].bulletSpeed, gunList[currentGun].bulletDelay, gunList[currentGun].bulletPrecision));
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        //Gizmos.DrawLine(groundChecker.position, new Vector3(groundChecker.position.x, groundChecker.position.y - groundCheckDistance, groundChecker.position.z));
        Gizmos.DrawWireSphere(groundChecker.position, groundCheckDistance);
    }

    void SwitchGun(int lastGun)
    {
        gunObjets[lastGun].SetActive(false);
        gunObjets[currentGun].SetActive(true);
    }

    bool IsGrounded()
    {
        bool grounded = Physics.CheckSphere(groundChecker.position, groundCheckDistance, LayerMask.GetMask("Ground"));
        if (grounded)
        {
            Debug.DrawLine(groundChecker.position, new Vector3(groundChecker.position.x, groundChecker.position.y - groundCheckDistance, groundChecker.position.z), Color.red);
        }
        else
        {
            Debug.DrawLine(groundChecker.position, new Vector3(groundChecker.position.x, groundChecker.position.y - groundCheckDistance, groundChecker.position.z), Color.blue);
        }
        return grounded;
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