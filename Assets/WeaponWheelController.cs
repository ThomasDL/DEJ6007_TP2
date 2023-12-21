using UnityEngine;
using UnityEngine.UI;

public class WeaponWheelController : MonoBehaviour
{
    [SerializeField] private Animator anim;
    private bool weaponWheelSelected;
    public static int weaponId;


    private PlayerController_test _playerController;

    private void Start()
    {
        _playerController = PlayerController_test.Instance;
        weaponWheelSelected = _playerController.selectingWeapon;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            _playerController.selectingWeapon = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        if (Input.GetKeyUp(KeyCode.Tab))
        {
            _playerController.selectingWeapon = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (_playerController.selectingWeapon)
        {
            anim.SetBool("openWeaponWheel", true);
        }
        else
        {
            anim.SetBool("openWeaponWheel", false);
        }
    }
}
