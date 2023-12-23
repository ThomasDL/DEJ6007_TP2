using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuPanelManager : MonoBehaviour
{
    [SerializeField] GameObject menuObject;
    [SerializeField] TextMeshProUGUI sensitivityValueText;
    public GameObject weaponWheel;

    bool isOpen = true;

    private void Start()
    {
        Time.timeScale = 0.01f;
    }

    public void OpenMenu()
    {
        if(isOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            menuObject.SetActive(false);
            weaponWheel.SetActive(true);
            isOpen = false;
            Time.timeScale = 1f;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            menuObject.SetActive(true);
            weaponWheel.SetActive(false);
            isOpen = true;
            Time.timeScale = 0.01f;
        }
    }
    public void SensitivityChange(float newSensitivity)
    {
        sensitivityValueText.text = newSensitivity.ToString();
        PlayerController_test.Instance.GetComponentInChildren<MouseLook>().mouseSensitivity = newSensitivity;
    }
}
