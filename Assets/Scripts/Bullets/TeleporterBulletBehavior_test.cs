using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterBulletBehavior_test : MonoBehaviour
{
    private CharacterController _characterController_Unity;

    private GameObject translocatorScreen;
    [SerializeField] private Camera translocatorBulletCamera;

    [SerializeField] private bool useAsGhost;

    private float _bulletSpeed = 32f;
    private Rigidbody _bullet_rb;
    public Transform groundChecker;
    float groundCheckRadius = 0.1f;

    public void GetTranslocatorScreenReference(GameObject reference)
    {
        translocatorScreen = reference;
    }


    [HideInInspector] public bool CheckCollisionWithFloor = false;

    //[SerializeField] private float travelTimeDestroy = 1.5f;
    
    //A single teleportBullet should exist at all time
    public static TeleporterBulletBehavior_test Instance;


    private void Awake()
    {
        //Only used in the real scene, not the physics simulation scene
        if (!useAsGhost)
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject); // Destroy the older instance
            }

            Instance = this; // Set the current instance as the Singleton instance
        }

        _bullet_rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        //Caching a reference to the CharacterController instead of doing it for each collision in the update
        _characterController_Unity = GameObject.Find("Player_test_achraf").GetComponent<CharacterController>();
        //Shoot_TP_Bullet();
    }

    // Update is called once per frame
    void Update()
    {
        TranslocatePlayer();
    }

    public void Shoot_TP_Bullet()
    {
        this._bullet_rb.AddForce(this.gameObject.transform.forward * _bulletSpeed, ForceMode.Impulse);
    }

    private void TranslocatePlayer()
    {
        if (Physics.CheckSphere(groundChecker.position, groundCheckRadius, LayerMask.GetMask("Ground")) && !useAsGhost)
        {
            translocatorScreen.SetActive(false);
            _characterController_Unity.enabled = false;
            _characterController_Unity.transform.position = groundChecker.position + new Vector3(0, _characterController_Unity.height / 2, 0);
            _characterController_Unity.enabled = true;
            Destroy(gameObject);
        }
        /*
        if (Physics.CheckSphere(groundChecker.position, groundCheckRadius, LayerMask.GetMask("Ground")) && !useAsGhost)
        {
            CheckCollisionWithFloor = true;
            translocatorBulletCamera.transform.rotation = Camera.main.transform.rotation;
            translocatorScreen.SetActive(true);
        }

        //if (Input.GetButtonDown("Fire2"))
        if (Input.GetKeyDown(KeyCode.E))
        {
            translocatorScreen.SetActive(false);
            Destroy(gameObject);
            _characterController_Unity.enabled = false;
            _characterController_Unity.transform.position = groundChecker.position + new Vector3(0, _characterController_Unity.height / 2, 0);
            _characterController_Unity.enabled = true;
        }
        //Temporary solution to destroy the bullet if it takes too long
        //Destroy(gameObject, travelTimeDestroy);
        */
    }
}
