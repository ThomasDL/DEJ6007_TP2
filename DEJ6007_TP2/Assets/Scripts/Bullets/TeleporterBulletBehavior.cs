using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleporterBulletBehavior : MonoBehaviour
{
    public Transform groundChecker;
    float groundCheckRadius = 0.1f;

    // Update is called once per frame
    void Update()
    {
        if (Physics.CheckSphere(groundChecker.position, groundCheckRadius,LayerMask.GetMask("Ground")))
        {
            CharacterController player = GameObject.Find("Player").GetComponent<CharacterController>();
            player.enabled = false;
            player.transform.position = groundChecker.position + new Vector3(0, player.height / 2, 0);
            player.enabled = true;
            Destroy(gameObject);
        }
    }
}
