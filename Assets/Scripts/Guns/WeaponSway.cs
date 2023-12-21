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

        SwayOffset();
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

    private void SwayOffset()
    {
        if (sway)
        {
            float aimingMultiplier = isAiming ? 0.2f : 1.0f; // Reduce effect to some % when aiming

            Vector3 invertLook = lookInput * -step * aimingMultiplier;
            invertLook.x = Mathf.Clamp(invertLook.x, -maxStepDistance, maxStepDistance);
            invertLook.y = Mathf.Clamp(invertLook.y, -maxStepDistance, maxStepDistance);

            swayPos = invertLook;
        }
        else
        {
            swayPos = Vector3.zero;
        }
    }

    private void SwayRotation()
    {
        if (swayRotation)
        {
            float aimingMultiplier = isAiming ? 0.2f : 1.0f;

            Vector2 invertLook = lookInput * -rotationStep * aimingMultiplier;
            invertLook.x = Mathf.Clamp(invertLook.x, -maxRotationStep, maxRotationStep);
            invertLook.y = Mathf.Clamp(invertLook.y, -maxRotationStep, maxRotationStep);
            swayEulerRot = new Vector3(invertLook.y, invertLook.x, invertLook.x);
        }
        else
        {
            swayEulerRot = Vector3.zero;
        }
    }


    private Quaternion externalRotation = Quaternion.identity;

    public void ApplyExternalRotation(Quaternion rotation)
    {
        externalRotation = rotation;
    }


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
            float incrementRate = _player.IsGrounded ? (walkInput.magnitude * bobExaggeration) : 1f;
            float stationaryIncrement = 0.5f;

            bool isWalking = walkInput != Vector2.zero;
            float aimingMultiplier = isAiming ? 0.05f : 1.0f;

            float increment = Time.deltaTime * ((isWalking && !isAiming) ? incrementRate : stationaryIncrement) * aimingMultiplier + 0.01f;
            speedCurve += increment;

            speedCurve %= 2 * Mathf.PI;

            float stationaryBobMultiplier = 0.2f;
            Vector3 adjustedBobLimit = (isWalking && !isAiming) ? bobLimit : bobLimit * stationaryBobMultiplier;
            adjustedBobLimit *= aimingMultiplier; // Apply reduced effect when aiming

            bobPosition.x = (curveCos * adjustedBobLimit.x * (_player.IsGrounded ? 1 : 0)) - (walkInput.x * travelLimit.x);
            bobPosition.y = (curveSin * adjustedBobLimit.y) - (Input.GetAxis("Vertical") * travelLimit.y);
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
            float aimingMultiplier = isAiming ? 0.05f : 1.0f;
            float pitchMultiplier = (walkInput != Vector2.zero) ? 1f : 0.1f;
            float frequency = (walkInput != Vector2.zero) ? 2f : 0.5f;

            bobEulerRotation.x = multiplier.x * (Mathf.Sin(frequency * speedCurve)) * pitchMultiplier * aimingMultiplier;
            bobEulerRotation.y = (walkInput != Vector2.zero) ? multiplier.y * curveCos * aimingMultiplier : 0;
            bobEulerRotation.z = (walkInput != Vector2.zero) ? multiplier.z * curveCos * walkInput.x * aimingMultiplier : 0;
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
}
