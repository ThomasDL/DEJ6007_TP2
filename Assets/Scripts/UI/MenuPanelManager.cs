using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuPanelManager : MonoBehaviour
{
    [SerializeField] GameObject menuObject;
    [SerializeField] TextMeshProUGUI sensitivityValueText;
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ResumeGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        menuObject.SetActive(false);
        Time.timeScale = 1f;
    }
    public void OpenMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        menuObject.SetActive(true);
        Time.timeScale = 0f;
    }
    public void SensitivityChange(float newSensitivity)
    {
        sensitivityValueText.text = newSensitivity.ToString();
        PlayerController_test.Instance.GetComponentInChildren<MouseLook>().mouseSensitivity = newSensitivity;
    }
}
