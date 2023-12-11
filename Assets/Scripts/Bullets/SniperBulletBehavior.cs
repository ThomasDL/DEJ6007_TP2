using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperBulletBehavior : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Head")) collision.collider.GetComponentInParent<EnemyBase>().DamageEnemy(10);
        else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemies")) collision.collider.GetComponentInParent<EnemyBase>().DamageEnemy(5);
        else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) Destroy(gameObject);
    }
}
