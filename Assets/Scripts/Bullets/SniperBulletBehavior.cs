using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperBulletBehavior : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Head"))
        {
            collision.collider.GetComponentInParent<EnemyBase>().DamageEnemy(12);
            Destroy(gameObject);
        }
        else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemies")) 
        {
            collision.collider.GetComponentInParent<EnemyBase>().DamageEnemy(3);
            Destroy(gameObject);
        } 
        else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) Destroy(gameObject);
    }
}
