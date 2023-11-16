using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunBulletBehavior : MonoBehaviour
{
    int collisionCounter;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) collisionCounter++;
        if (collisionCounter == 2) Destroy(gameObject);
    }
}
