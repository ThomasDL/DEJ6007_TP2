using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UI_Manager : MonoBehaviour
{
    private PlayerController_test _player;
    [SerializeField] private TextMeshProUGUI healthText = default;

    #region Health Bar Image

    [SerializeField] private Image healthBarForeground;
    [SerializeField] private float updateFillSpeed = 2.0f;

    private float targetHealthPercentage = 1.0f;
    private float maxHealth;
    private float currentHealth;

    #endregion

    #region Controls
    [SerializeField] private Image controlsImage;
    #endregion

    #region Body Position
    [SerializeField] private Image bodyPositionImage;
    public static Action<bool> OnCrouch;
    #endregion

    private void OnEnable()
    {
        PlayerController_test.OnDamage += UpdateHealth;
        PlayerController_test.OnHeal += UpdateHealth;
        OnCrouch += SetBodyPositionActive;
    }

    private void OnDisable()
    {
        PlayerController_test.OnDamage -= UpdateHealth;
        PlayerController_test.OnHeal -= UpdateHealth;
        OnCrouch -= SetBodyPositionActive;
    }

    private void Start()
    {
        // Caching a reference to PlayerController_test
        _player = PlayerController_test.Instance;

        // Get the maxHealth from the PlayerController_test
        maxHealth = _player.GetMaxHealth();

        UpdateHealth(maxHealth);

        // Setting the UI active
        controlsImage.gameObject.SetActive(false);
        //bodyPositionImage.enabled = _player.IsCrouching;
    }

    private void UpdateHealth(float healthAmount)
    {
        healthText.text = healthAmount.ToString("00");
        targetHealthPercentage = healthAmount / maxHealth;
    }

    private void Update()
    {
        // Smoothly transition the health bar
        healthBarForeground.fillAmount = Mathf.Lerp(healthBarForeground.fillAmount, targetHealthPercentage, updateFillSpeed * Time.deltaTime);

        //Enable or disabled input controls information
        if(Input.GetKeyDown(KeyCode.O))
        {
            controlsImage.gameObject.SetActive(!controlsImage.gameObject.activeInHierarchy);
        }

        if (_player.IsCrouching)
        {
            bodyPositionImage.gameObject.SetActive(_player.IsCrouching);
        }
    }

    private void SetBodyPositionActive(bool state)
    {
        bodyPositionImage.enabled = state;
    }
}
