using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    private PlayerController_test _player;

    [Header("Settings")]
    [SerializeField] bool sway = true;
    [SerializeField] bool swayRotation = true;
    [SerializeField] bool bobOffset = true;
    [SerializeField] bool bobSway = true;

    [Header("Sway")]
    [SerializeField] private float step = 0.01f;
    [SerializeField] private float maxStepDistance = 0.06f;
    private Vector3 swayPos;

    [Header("Sway Rotation")]
    [SerializeField] private float rotationStep = 4f;
    [SerializeField] private float maxRotationStep = 5f;
    private Vector3 swayEulerRot;

    [SerializeField] private float smooth = 10f;
    private float smoothRot = 12f;

    [Header("Bobbing")]
    [SerializeField] private float speedCurve;
    private float curveSin { get => Mathf.Sin(speedCurve); }
    private float curveCos { get => Mathf.Cos(speedCurve); }

    [SerializeField] private Vector3 travelLimit = Vector3.one * 0.025f;
    [SerializeField] private Vector3 bobLimit = Vector3.one * 0.01f;
    private Vector3 bobPosition;

    [SerializeField] private float bobExaggeration;

    [Header("Bob Rotation")]
    [SerializeField] private Vector3 multiplier;
    private Vector3 bobEulerRotation;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    [Header("Aiming")]
    public Vector3 normalPosition; // The normal resting position of the weapon
    public Vector3 aimingPosition; // The position of the weapon when aiming
    private bool isAiming; // A flag to indicate if the player is aiming


    // Start is called before the first frame update
    void Start()
    {
        // Caching a reference to PlayerController_test
        _player = PlayerController_test.Instance;

        // Store the initial local position and rotation
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();

        //if (!isAiming)
        //{
        //    Sway();
        //    SwayRotation();
        //    BobOffset();
        //    BobRotation();
        //}

        Sway();
        SwayRotation();
        BobOffset();
        BobRotation();

        CompositePositionRotation();
    }


    private Vector2 walkInput;
    private Vector2 lookInput;

    private void GetInput()
    {
        walkInput.x = Input.GetAxis("Horizontal");
        walkInput.y = Input.GetAxis("Vertical");
        walkInput = walkInput.normalized;

        lookInput.x = Input.GetAxis("Mouse X");
        lookInput.y = Input.GetAxis("Mouse Y");
    }


    private void Sway()
    {
        if (sway)
        {
            Vector3 invertLook = lookInput * -step;
            invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
            invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);

            swayPos = invertLook;
        }
        else
        {
            swayPos = Vector3.zero;
            return;
        }
    }

    private void SwayRotation()
    {
        if (swayRotation)
        {
            Vector2 invertLook = lookInput * -rotationStep;
            invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep, maxRotationStep);
            invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);
            swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
        }
        else
        {
            swayEulerRot = Vector3.zero;
            return;
        }
    }

    private Quaternion externalRotation = Quaternion.identity;

    public void ApplyExternalRotation(Quaternion rotation)
    {
        externalRotation = rotation;
    }

    //private void CompositePositionRotation()
    //{
    //    // Calculate sway and bobbing rotation
    //    Quaternion swayBobRotation = Quaternion.Euler(swayEulerRot + bobEulerRotation);

    //    // Combine the initial rotation with sway/bobbing rotation and external rotation (from prevent clip logic)
    //    Quaternion combinedRotation = initialLocalRotation * swayBobRotation * externalRotation;

    //    // Apply combined rotation
    //    transform.localRotation = Quaternion.Slerp(transform.localRotation, combinedRotation, Time.deltaTime * smoothRot);

    //    // Apply position changes
    //    Vector3 targetPosition = initialLocalPosition + swayPos + bobPosition;
    //    transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * smooth);
    //}

    private void CompositePositionRotation()
    {
        // Calculate sway and bobbing rotation
        Quaternion swayBobRotation = Quaternion.Euler(swayEulerRot + bobEulerRotation);

        // Combine the initial rotation with sway/bobbing rotation and external rotation
        Quaternion combinedRotation = initialLocalRotation * swayBobRotation * externalRotation;

        // Apply combined rotation
        transform.localRotation = Quaternion.Slerp(transform.localRotation, combinedRotation, Time.deltaTime * smoothRot);

        // Interpolate between normal position and aiming position based on isAiming
        Vector3 targetPosition = isAiming ? aimingPosition : normalPosition;
        targetPosition += swayPos + bobPosition;

        // Apply position changes
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * smooth);
    }

    private void BobOffset()
    {
        if (bobOffset)
        {
            // Determine the increment rate of speedCurve variable, based on whether the player is moving
            float incrementRate = _player.IsGrounded ? (walkInput.magnitude * bobExaggeration) : 1f;

            // When the player is not moving, we use a smaller increment for a slower breathing effect
            float stationaryIncrement = 0.5f; // Adjust this value to control the breathing speed
            
            bool isWalking = walkInput != Vector2.zero;
            //float increment = Time.deltaTime * (walkInput != Vector2.zero ? incrementRate : stationaryIncrement) + 0.01f;
            float increment = Time.deltaTime * ((isWalking && !isAiming) ? incrementRate : stationaryIncrement) + 0.01f;

            speedCurve += increment;

            // Wrap speedCurve to prevent it from growing indefinitely
            speedCurve %= 2 * Mathf.PI;

            // Adjust the bobLimit for a more subtle effect when not moving
            float stationaryBobMultiplier = 0.2f; // Adjust this value to control the breathing amplitude
            //Vector3 adjustedBobLimit = walkInput != Vector2.zero ? bobLimit : bobLimit * stationaryBobMultiplier;
            Vector3 adjustedBobLimit = (isWalking && !isAiming) ? bobLimit : bobLimit * stationaryBobMultiplier;

            // Calculate the bob position using the adjusted limits and actual walkInput direction
            // Negating walkInput values to move in the opposite direction of player movement
            bobPosition.x = (curveCos * adjustedBobLimit.x * (_player.IsGrounded ? 1 : 0)) - (walkInput.x * travelLimit.x);
            bobPosition.y = (curveSin * adjustedBobLimit.y) - (Input.GetAxis("Vertical") * travelLimit.y);
            //bobPosition.z = curveCos * adjustedBobLimit.z * (_player.IsGrounded ? walkInput.y : 0);
            bobPosition.z = -(walkInput.y * travelLimit.z);
        }
        else
        {
            bobPosition = Vector3.zero;
        }
    }

    private void BobRotation()
    {
        if (bobSway)
        {
            // Slowing down the bob effect when not moving by reducing the multiplier for the x-axis.
            float pitchMultiplier = (walkInput != Vector2.zero) ? 1f : 0.1f; // Reduced to 10% when not moving

            // Using a smaller value for frequency to slow down the sine wave for the breathing effect
            float frequency = (walkInput != Vector2.zero) ? 2f : 0.5f; // Slower frequency for breathing

            bobEulerRotation.x = multiplier.x * (Mathf.Sin(frequency * speedCurve)) * pitchMultiplier;
            bobEulerRotation.y = (walkInput != Vector2.zero) ? multiplier.y * curveCos : 0;

            // For the z-axis, you might want to apply similar logic as the x-axis if you want to slow down the roll effect as well.
            bobEulerRotation.z = (walkInput != Vector2.zero) ? multiplier.z * curveCos * walkInput.x : 0;
        }
        else
        {
            bobEulerRotation = Vector3.zero;
        }
    }

    public void SetAiming(bool aiming)
    {
        isAiming = aiming;
    }

    //PREVIOUSLY WORKING BUT BASIC CODE

    //private void CompositePositionRotation()
    //{
    //    // Apply sway and bobbing as offsets to the initial position and rotation
    //    Vector3 targetPosition = initialLocalPosition + swayPos + bobPosition;
    //    Quaternion targetRotation = initialLocalRotation * Quaternion.Euler(swayEulerRot + bobEulerRotation) * externalRotation;

    //    transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * smooth);
    //    transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * smoothRot);
    //}


    //private void BobOffset()
    //{
    //    if (bobOffset)
    //    {
    //        //speedCurve += Time.deltaTime * (_player.IsGrounded ? (Input.GetAxis("Horizontal") + Input.GetAxis("Vertical")) * bobExaggeration : 1f) + 0.01f;
    //        speedCurve += Time.deltaTime * (_player.IsGrounded ? (walkInput.x + walkInput.y) * bobExaggeration : 1f) + 0.01f;

    //        bobPosition.x = (curveCos * bobLimit.x * (_player.IsGrounded ? 1 : 0)) - (walkInput.x * travelLimit.x);
    //        bobPosition.y = (curveSin * bobLimit.y) - (Input.GetAxis("Vertical") * travelLimit.y);
    //        bobPosition.z = -(walkInput.y * travelLimit.z);
    //    }
    //    else
    //    {
    //        bobPosition = Vector3.zero;
    //        return;
    //    }
    //}

    //private void BobRotation()
    //{
    //    if (bobSway)
    //    {
    //        bobEulerRotation.x = (walkInput != Vector2.zero ? multiplier.x * (Mathf.Sin(2 * speedCurve)) : multiplier.x * (Mathf.Sin(2 * speedCurve) / 10));
    //        bobEulerRotation.y = (walkInput != Vector2.zero ? multiplier.y * curveCos : 0);
    //        bobEulerRotation.z = (walkInput != Vector2.zero ? multiplier.z * curveCos * walkInput.x : 0);
    //    }
    //    else
    //    {
    //        bobEulerRotation = Vector3.zero;
    //        return;
    //    }
    //}
}
