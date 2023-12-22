using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController_test : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Camera weaponCamera;
    [SerializeField] private Transform groundChecker;
    [SerializeField] private GameObject capsuleCharacter;
    [SerializeField] private GameObject trajectoryLine;
    [SerializeField] private GameObject sniperScope;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] audioClips;

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
    [SerializeField] private float walkingJumpForce = 8f;
    [SerializeField] private float sprintingJumpForce = 11f;
    [SerializeField] private float crouchingJumpForce = 6f;
    private float jumpForce;
    private bool wasGroundedLastFrame;

    // Bool to check if player is currently moving.
    public static bool IsMoving
    {
        get
        {
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    [Header("Coyote Time")]
    private float coyoteTimeDuration = 15f / 60f; // A coyote jump timeframe equivalent to 15 frames at 60 FPS
    private float timeSinceUngrounded = 0f;
    private bool justJumped = false;

    Vector3 verticalVelocity;
    [SerializeField] private float groundCheckDistance = 0.25f;
    private bool isGrounded;

    public bool IsGrounded { get { return isGrounded; } }

    [Header("Modular movement options")]
    [SerializeField] private bool canSprint = true; // Enable sprint logic
    [SerializeField] private bool canJump = true; // Enable jump logic
    [SerializeField] private bool canCrouch = true; // Enable crouch logic
    [SerializeField] private bool canUseHeadBob = true; // Enable Head bob logic
    [SerializeField] private bool willSlideOnSlopes = true; // Enable slopes logic
    [SerializeField] private bool canZoom = true; // Enable zoom logic

    // Controls
    private KeyCode sprintKey = KeyCode.LeftShift;
    private KeyCode jumpKey = KeyCode.Space;
    private KeyCode zoomKey = KeyCode.Mouse1;


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
    public bool IsCrouching
    {
        get { return isCrouching; }
    }

    public float CrouchHeight { get { return crouchHeight; } }
    public float StandingHeight { get { return standingHeight; } }

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

    private bool IsSliding
    {
        get
        {
            if (isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 1f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > controller.slopeLimit;
            }
            else return false;
        }
    }

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

    [HideInInspector] public bool selectingWeapon = false;

    // Easier reference spot for the translocator screen for now
    [SerializeField] private GameObject translocatorScreen;

    private DynamicCrosshair weaponCrosshair;

    #endregion

    #region WeaponAimZoom

    [Header("Zoom parameters")]
    [SerializeField] private float timeToZoom = 0.3f;
    private bool startedAiming = false;

    // could be moved to a gun variable, so we can have different aiming
    private float defaultFOV;
    private Coroutine zoomRoutine;


    private Vector3 hitPointNormal;

    #endregion

    #region WeaponClippingPrevention

    [Header("Weapon clipping prevention")]
    [SerializeField] private GameObject weaponHolder;
    [SerializeField] private GameObject clipProjector;
    [SerializeField] private float checkDistance;

    [Tooltip("Layers to be ignored when preventing clipping")]
    [SerializeField] private LayerMask ignoredLayers;

    [Tooltip("Set the rotation to prevent clipping. Default is (0, -90, 0)")]
    [SerializeField] private Vector3 noShootingPosition = new Vector3(0, -90, 0);

    // Threshold compared to weapon rotation when near an obstacle
    [SerializeField] private float shootDisablingThreshold = 0.10f;

    private float lerpPosition;
    RaycastHit hit;
    #endregion

    #region WeaponRecoil
    // Variables for Recoil
    #endregion

    #region Health System

    [Header("Health Parameters")]
    [SerializeField] private float maxHealth = 100f;

    [SerializeField] private float timeBeforeRegenStarts = 3f;
    [SerializeField] private float healthValueIncrement = 1f;
    [SerializeField] private float healthTimeIncrement = 0.1f;

    private float currentHealth;

    // We regenerate health with the coroutine that we cache further down in the script into this variable
    // Regen works like classic modern FPS : small increments after not receiving damage for a while
    private Coroutine regeneratingHealth;

    public static Action<float> OnTakeDamage;
    public static Action<float> OnDamage;
    public static Action<float> OnHeal;

    private GameSession gameOverScreen;

    #endregion

    // Player is a Singleton = easier access to the player instance
    // directly via this static property instead of GameObject.Find()
    // From other script, use : "player = PlayerController_test.Instance.gameObject;"
    public static PlayerController_test Instance { get; private set; }
    // Alternative : GameObject.FindWithTag("Player") is more efficient than GameObject.Find("Player").

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        defaultYPos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;
        currentHealth = maxHealth; // Initialize current health to max at start
        wasGroundedLastFrame = isGrounded;
    }

    private void OnEnable()
    {
        OnTakeDamage += ApplyDamage;
    }

    private void OnDisable()
    {
        OnTakeDamage -= ApplyDamage;
    }

    private void Start()
    {
        SetPlayerActionsState(true);
        isGrounded = CheckIfGrounded();
        canShoot = true;
        crouchSpeed = walkSpeed * 0.35f;
        trajectoryLine.SetActive(false);
        PlayerUI_Manager.OnCrouch(isCrouching);
        weaponCrosshair = FindObjectOfType<DynamicCrosshair>();
        gameOverScreen = FindObjectOfType<GameSession>();
        selectingWeapon = false;
        audioSource.pitch = 1f;
    }

    void Update()
    {
        // Groundcheck
        isGrounded = CheckIfGrounded();

        PreventClip();

        HandleFiring();

        HandleGunSwitch();

        if (CanMove)
        {
            HandleMovementInput();
            if (canJump) HandleJump();
            if (canCrouch) HandleCrouch();
            if (canUseHeadBob) HandleHeadBob();
            if (canZoom) HandleZoom();
        }

        if (!canShoot || currentGun == 2)
        {
            weaponCrosshair.SetCrosshairVisibility(false);
        }
        else
        {
            weaponCrosshair.SetCrosshairVisibility(true);
        }

        wasGroundedLastFrame = isGrounded;
    }

    #region Movement methods
    private void HandleMovementInput()
    {
        // Ground movement code
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        startedSprinting = Input.GetKeyDown(sprintKey);

        if (isGrounded)
        {
            moveSpeed = isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed;

            // Check if the player has just landed
            if (!wasGroundedLastFrame)
            {
                verticalVelocity.y = 0f;
            }
        }

        if (willSlideOnSlopes && IsSliding)
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }
        else
        {
            moveDirection = transform.right * x + transform.forward * z;
        }

        controller.Move(moveSpeed * Time.deltaTime * moveDirection);

        wasGroundedLastFrame = isGrounded;
    }

    private void HandleJump()
    {
        jumpForce = isCrouching ? crouchingJumpForce : IsSprinting ? sprintingJumpForce : walkingJumpForce;
        if (isGrounded)
        {
            // Reset consumed coyote time and justJumped flag
            timeSinceUngrounded = 0f;
            if (justJumped == true) justJumped = false;

            // Perform regular jump when grounded
            if (ShouldJump)
            {
                //Debug.Log("jumpForce = " + jumpForce);
                justJumped = true;
                verticalVelocity.y = jumpForce;
            }
        }
        else // Apply gravity and update coyote time and justJumped flag
        {
            verticalVelocity.y += gravity * Time.deltaTime;

            timeSinceUngrounded += Time.deltaTime;
            bool canCoyoteJump = (timeSinceUngrounded <= coyoteTimeDuration) && (justJumped == false);

            // Perform jump using coyote time
            if (ShouldJump && canCoyoteJump && !justJumped)
            {
                justJumped = true;
                //Debug.Log("JUMP using COYOTE TIME + jumpForce = " + jumpForce);
                verticalVelocity.y = jumpForce;
            }
        }

        controller.Move(verticalVelocity * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        // Player can exit crouching either by pressing crouchKey again or when he starts running
        if ((!isCrouching && ShouldCrouch) || (isCrouching && (ShouldCrouch || startedSprinting)))
        {
            StartCoroutine(CrouchStand());
        }
    }

    private IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 2f))
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
        PlayerUI_Manager.OnCrouch(isCrouching);
        duringCrouchAnimation = false;
    }

    private void HandleHeadBob()
    {
        if (!isGrounded) return;

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }

    private void SetPlayerActionsState(bool state)
    {
        CanMove = state;
        canJump = state;
        canCrouch = state;
        canZoom = state;
        canSprint = state;
        canShoot = state;
    }

    bool CheckIfGrounded()
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
    #endregion

    #region Zoom Methods
    private void HandleZoom()
    {
        if (Input.GetKeyDown(zoomKey) && canShoot)
        {
            startedAiming = true;
            EnableZoom();
        }

        if (startedAiming && (Input.GetKeyUp(zoomKey) || !canShoot || selectingWeapon || lastGun != currentGun))
        {
            startedAiming = false;
            DisableZoom();
        }
    }

    private void EnableZoom()
    {
        if (zoomRoutine != null)
        {
            StopCoroutine(zoomRoutine);
            zoomRoutine = null;
        }

        zoomRoutine = StartCoroutine(ToggleZoom(true));
        weaponHolder.GetComponent<WeaponSway>().SetAiming(true);
    }

    private void DisableZoom()
    {
        if (zoomRoutine != null)
        {
            StopCoroutine(zoomRoutine);
            zoomRoutine = null;
        }

        zoomRoutine = StartCoroutine(ToggleZoom(false));
        weaponHolder.GetComponent<WeaponSway>().SetAiming(false);
    }

    private IEnumerator ToggleZoom(bool isEnter)
    {
        float targetFOV = isEnter ? gunList[currentGun].customZoomFOV : defaultFOV;

        if(!isEnter && currentGun == 1)
        {
            sniperScope.SetActive(false);
            //gunObjets[currentGun].GetComponent<MeshRenderer>().enabled = true;
            gunObjets[currentGun].SetActive(true);
        }

        if (currentGun == 2)
        {
            trajectoryLine.SetActive(!isEnter);
        }


        float playerCameraStartingFOV = playerCamera.fieldOfView;
        float weaponCameraStartingFOV = weaponCamera.fieldOfView;

        float timeElapsed = 0;

        while (timeElapsed < timeToZoom)
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCameraStartingFOV, targetFOV, timeElapsed / timeToZoom);
            weaponCamera.fieldOfView = Mathf.Lerp(weaponCameraStartingFOV, targetFOV, timeElapsed / timeToZoom);

            timeElapsed += Time.deltaTime;

            yield return null;
        }

        playerCamera.fieldOfView = targetFOV;
        weaponCamera.fieldOfView = targetFOV;

        if (isEnter && currentGun == 1)
        {
            sniperScope.SetActive(true);
            gunObjets[currentGun].SetActive(false);
        }

        zoomRoutine = null;
    }
    #endregion

    #region Health System Methods

    private void ApplyDamage(float damage)
    {
        currentHealth -= damage;
        audioSource.pitch = UnityEngine.Random.Range(0.95f, 1.35f);
        audioSource.PlayOneShot(audioClips[0]);

        // (if not null "?") Invokes this event to notify other systems (like UI)
        // that are subscribed to this event.
        OnDamage?.Invoke(currentHealth);

        if (currentHealth <= 0)
        {
            KillPlayer();
        }
        else if (regeneratingHealth != null)
        {
            StopCoroutine(regeneratingHealth);
        }

        // Starts the health regeneration coroutine.
        regeneratingHealth = StartCoroutine(RegenerateHealth());
    }

    private void KillPlayer()
    {
        currentHealth = 0;
        audioSource.PlayOneShot(audioClips[1]);
        audioSource.pitch = 0.25f;
        audioSource.PlayOneShot(audioClips[0]);
        OnDamage?.Invoke(currentHealth);

        // Stops health regen if the player is dead.
        if (regeneratingHealth != null)
        {
            StopCoroutine(regeneratingHealth);
        }
        // Implement what happens when the player dies

        // Display death screen with replay button.
        SetPlayerActionsState(false);
        gameOverScreen.DisplayGameOverScreen(true);
        Debug.Log("Dead");
    }

    // Coroutine that gradually restores the player's health over time after a delay.
    private IEnumerator RegenerateHealth()
    {
        // Frame 1: Starts the coroutine.
        // Waits for 'timeBeforeRegenStarts' seconds before executing the next line.
        // This is a pause, not a block; other game operations continue.
        yield return new WaitForSeconds(timeBeforeRegenStarts);

        // Frame after delay: The wait is over.
        // Sets up another wait time for health increment.
        WaitForSeconds timeToWait = new WaitForSeconds(healthTimeIncrement);

        // Enters a loop, executed once per frame.
        while (currentHealth < maxHealth)
        {
            // Increases the player's health by 'healthValueIncrement'.
            currentHealth += healthValueIncrement;

            // Ensures health does not exceed the maximum limit.
            currentHealth = Mathf.Min(currentHealth, maxHealth);

            // (if not null "?") Invokes this event to update
            // any listeners (like UI) about the health change.
            OnHeal?.Invoke(currentHealth);

            // Waits for 'healthTimeIncrement' seconds before next loop iteration.
            // Again, this wait is non-blocking.
            yield return timeToWait;
        }

        // Once the loop is complete (health is fully regenerated), this line is reached.
        // Resets the coroutine reference, indicating that regeneration is complete.
        regeneratingHealth = null;
    }
    public float GetMaxHealth()
    {
        return maxHealth;
    }
    #endregion

    #region Weapon handling methods
    private void HandleGunSwitch()
    {
        if (Input.GetButtonDown("Gun0")) currentGun = 0;
        if (Input.GetButtonDown("Gun1")) currentGun = 1;
        if (Input.GetButtonDown("Gun2")) currentGun = 2;
        if (Input.GetButtonDown("Gun3")) currentGun = 3;
        if (Input.GetButtonDown("Gun4")) currentGun = 4;
        if (lastGun != currentGun) SwitchGun(lastGun);
        lastGun = currentGun;
    }

    public void PlaySoundOnPlayer(int audioClipNumber, float volumeScale)
    {
        audioSource.PlayOneShot(audioClips[audioClipNumber], volumeScale);
    }

    private void HandleFiring()
    {
        // Disable shooting if the weapon is rotated significantly
        if (lerpPosition >= shootDisablingThreshold || selectingWeapon)
        {
            weaponCrosshair.SetCrosshairVisibility(false);
            canShoot = false;
        }
        else if (lerpPosition < shootDisablingThreshold || !selectingWeapon)
        {
            weaponCrosshair.SetCrosshairVisibility(false);
            canShoot = true;
        }

        // Shooting code
        delaySinceFiring += Time.deltaTime;

        if (Input.GetButtonDown("Fire1") && canShoot && delaySinceFiring > gunList[currentGun].gunCooldown)
        {
            delaySinceFiring = 0f;
            StartCoroutine(ShootBullet(gunList[currentGun].bulletAmount, gunList[currentGun].bulletSpeed,
                                        gunList[currentGun].bulletUpImpusle, gunList[currentGun].bulletDelay,
                                         gunList[currentGun].bulletPrecision));
        }
    }

    void SwitchGun(int lastGun)
    {
        gunObjets[lastGun].SetActive(false);
        gunObjets[currentGun].SetActive(true);

        if (currentGun != 2)
        {
            trajectoryLine.SetActive(false);
        }
        else
        {
            trajectoryLine.SetActive(true);
        }

        if (currentGun != 1 && sniperScope.activeSelf) sniperScope.SetActive(false);

        DisableZoom();
    }

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
        //weapon.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(newDirection), lerpPosition);

        Quaternion clipRotation = Quaternion.Lerp(Quaternion.Euler(Vector3.zero), Quaternion.Euler(noShootingPosition), lerpPosition);
        weaponHolder.GetComponent<WeaponSway>().ApplyExternalRotation(clipRotation);
    }

    // !!!!!!
    // Rework needed here to make bullets a general (abstract or interface ?) class
    // with each bullet type inherinting from it and then overloading a Shoot() method
    // that handles the behavior of the bullet differently. BulletSpeed goes there too.
    //
    // Gun class should only handle gun behavior such as firing rate, ammo,
    // number of bullet to instantiate, etc.
    // !!!!!!
    IEnumerator ShootBullet(int amount, float speed, float upImpusle, float delay, float precision)
    {
        audioSource.PlayOneShot(gunList[currentGun].gunSound);
        for (int i = 0; i < amount; i++)
        {
            var bulletObject = Instantiate(gunList[currentGun].bulletPrefab, gunPoint.position, playerCamera.transform.rotation);

            // Special behavior for Teleporter Gun
            if (currentGun == 2)
            {
                bulletObject.GetComponent<TeleporterBulletBehavior_test>().GetTranslocatorScreenReference(translocatorScreen);
                bulletObject.GetComponent<TeleporterBulletBehavior_test>().Shoot_TP_Bullet();
            }
            else
            {
                bulletObject.GetComponent<Rigidbody>().AddForce(bulletObject.transform.forward * speed + UnityEngine.Random.insideUnitSphere * precision, ForceMode.Impulse);
                bulletObject.GetComponent<Rigidbody>().AddForce(bulletObject.transform.up * upImpusle, ForceMode.Impulse);
            }

            yield return new WaitForSeconds(delay);
        }
    }

    #endregion



    #region Helper / debug functions
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        //Gizmos.DrawLine(groundChecker.position, new Vector3(groundChecker.position.x, groundChecker.position.y - groundCheckDistance, groundChecker.position.z));
        Gizmos.DrawWireSphere(groundChecker.position, groundCheckDistance);
    }
    #endregion
}

[System.Serializable]
public class Gun_test
{
    public string gunName;
    public int bulletAmount;
    // 20 = slow, 200 = fast
    public float bulletSpeed;
    public float bulletUpImpusle;
    public float bulletDelay;
    public float gunCooldown;
    // 0 = super precise, 20f = very imprecise
    public float bulletPrecision;
    public GameObject bulletPrefab;
    public AudioClip gunSound;
    public float customZoomFOV;
}