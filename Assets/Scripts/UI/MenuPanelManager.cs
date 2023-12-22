using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuPanelManager : MonoBehaviour
{
    [SerializeField] GameObject menuObject;
    [SerializeField] TextMeshProUGUI sensitivityValueText;

    bool isOpen = true;

    public void OpenMenu()
    {
        if(isOpen)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            menuObject.SetActive(false);
            isOpen = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            menuObject.SetActive(true);
            isOpen = true;
        }
    }
    public void SensitivityChange(float newSensitivity)
    {
        sensitivityValueText.text = newSensitivity.ToString();
        PlayerController_test.Instance.GetComponentInChildren<MouseLook>().mouseSensitivity = newSensitivity;
    }
}
