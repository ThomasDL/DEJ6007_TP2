using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponWheelButtonController : MonoBehaviour
{
    [SerializeField] private int id;
    private int lastSelectedWeapon_Id;
    private Animator anim;
    [SerializeField] private string itemName;
    [SerializeField] private TextMeshProUGUI itemText;
    [SerializeField] private Image selectedItem;
    private bool selected = false;
    [SerializeField] private Sprite icon;
    [SerializeField] private AudioClip buttonSelected;

    private PlayerController_test _playerController;


    private void Start()
    {
        _playerController = PlayerController_test.Instance;
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        UpdateWeaponUI();
    }

    public void Selected()
    {
        //selected = true;
        //_playerController.currentGun = id;
    }

    public void Deselected()
    {
        selected = false;
    }

    public void HoverEnter()
    {
        _playerController.GetComponent<AudioSource>().PlayOneShot(buttonSelected);
        selected = true;
        _playerController.currentGun = id;

        anim.SetBool("Hover", true);
        itemText.text = itemName;

    }

    public void HoverExit()
    {
        selected = false;
        anim.SetBool("Hover", false);
        itemText.text = "";
    }

    public void UpdateWeaponUI()
    {
        if (selected)
        {
            selectedItem.sprite = icon;
            selectedItem.SetNativeSize();
        }
    }
    private void OnEnable()
    {
        PlayerController_test.onGunChanged += PlayerController_test_onGunChanged;
    }
    private void OnDisable()
    {
        PlayerController_test.onGunChanged -= PlayerController_test_onGunChanged;
    }
    private void PlayerController_test_onGunChanged(int gunID)
    {
        if (gunID == id)
        {
            selectedItem.sprite = icon;
            selectedItem.SetNativeSize();
        }
    }
}
