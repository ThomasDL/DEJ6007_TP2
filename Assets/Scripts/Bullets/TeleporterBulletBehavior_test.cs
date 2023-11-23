using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterBulletBehavior_test : MonoBehaviour
{
    private PlayerController_test _playerController;
    private CharacterController _characterController_Unity;

    [SerializeField] private bool useAsGhost;

    [HideInInspector] public static float _bulletSpeed;
    private Gun_test _currentGun;
    private Rigidbody _bullet_rb;
    public Transform groundChecker;
    float groundCheckRadius = 0.1f;


    [HideInInspector] public bool CheckCollisionWithFloor = false;

    [SerializeField] private float travelTimeDestroy = 1.5f;
    
    //Only one teleportBullet should exist in the real scene
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

        _playerController = GameObject.Find("Player_achraf").GetComponent<PlayerController_test>();

        _currentGun = _playerController.gunList[_playerController.currentGun];
        _bulletSpeed = _currentGun.bulletSpeed;

        //Caching a reference to the CharacterController instead of doing it for each collision in the update
        _characterController_Unity = GameObject.Find("Player_achraf").GetComponent<CharacterController>();


        //Shoot_TP_Bullet();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("TP_BulletBehavior gunList[currentGun].bulletSpeed = " + _bulletSpeed);
        if (Physics.CheckSphere(groundChecker.position, groundCheckRadius,LayerMask.GetMask("Ground")) && !useAsGhost)
        {
            _characterController_Unity.enabled = false;
            _characterController_Unity.transform.position = groundChecker.position + new Vector3(0, _characterController_Unity.height / 2, 0);
            _characterController_Unity.enabled = true;
            CheckCollisionWithFloor = true;
            Destroy(gameObject);
        }
        //Temporary solution to destroy the bullet if it take too long
        Destroy(gameObject, travelTimeDestroy);
    }

    public void Shoot_TP_Bullet()
    {
        //_bullet_rb.AddForce(this.gameObject.transform.forward * _bulletSpeed + Random.insideUnitSphere * _currentGun.bulletPrecision, ForceMode.Impulse);
        this._bullet_rb.AddForce(this.gameObject.transform.forward * _bulletSpeed, ForceMode.Impulse);
    }

    public void ShootBullet()
    {
        _bullet_rb.AddForce(this.gameObject.transform.forward * _bulletSpeed + Random.insideUnitSphere * _currentGun.bulletPrecision, ForceMode.Impulse);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(!useAsGhost) Debug.Log("Collision with : " + collision.gameObject.name);
    }
}
