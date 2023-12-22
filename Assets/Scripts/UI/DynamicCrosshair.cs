using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicCrosshair : MonoBehaviour
{
    private RectTransform crosshair;

    [SerializeField] private float restingSize;
    [SerializeField] private float maxSize;
    [SerializeField] private float crosshairResizeSpeed;
    private float currentSize = 65f;

    private void Start()
    {
        crosshair = GetComponent<RectTransform>();
    }

    private void Update()
    {
        ResizeCrosshair();
    }

    private void ResizeCrosshair()
    {

        // Check if player is currently moving and Lerp currentSize to the appropriate value.
        if (PlayerController_test.IsMoving)
        {
            currentSize = Mathf.Lerp(currentSize, maxSize, Time.deltaTime * crosshairResizeSpeed);
        }
        else
        {
            currentSize = Mathf.Lerp(currentSize, restingSize, Time.deltaTime * crosshairResizeSpeed);
        }

        // Set the reticle's size to the currentSize value.
        crosshair.sizeDelta = new Vector2(currentSize, currentSize);
    }

    // Public method to enable or disable the crosshair.
    public void SetCrosshairVisibility(bool isVisible)
    {
        crosshair.gameObject.SetActive(isVisible);
    }
}