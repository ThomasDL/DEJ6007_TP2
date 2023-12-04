using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunBulletBehavior : MonoBehaviour
{
    int collisionCounter;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) collisionCounter++;
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemies")) collision.collider.GetComponentInParent<EnemyBase>().DamageEnemy(1);
        if (collisionCounter == 1) Destroy(gameObject);
    }
}
