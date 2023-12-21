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
        selected = true;
        //lastSelectedWeapon_Id = id;
        _playerController.currentGun = id;
    }

    public void Deselected()
    {
        selected = false;
        //WeaponWheelController.weaponId = lastSelectedWeapon_Id;
    }

    public void HoverEnter()
    {
        //selected = true;
        //lastSelectedWeapon_Id = id;

        anim.SetBool("Hover", true);
        itemText.text = itemName;

        //_playerController.SwitchGun(lastSelectedWeapon_Id);
    }

    public void HoverExit()
    {
        anim.SetBool("Hover", false);
        itemText.text = "";
    }

    public void UpdateWeaponUI()
    {
        if (selected)
        {
            selectedItem.sprite = icon;
            selectedItem.SetNativeSize();
            itemText.text = itemName;
        }
    }
}
