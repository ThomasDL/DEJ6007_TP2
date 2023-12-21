using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperBulletBehavior : MonoBehaviour
{
    int headshotDamage = 12;
    int normalDamage = 5;
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Head"))
        {
            collision.collider.GetComponentInParent<EnemyBase>().DamageEnemy(headshotDamage + Random.Range(-1,1));
            Destroy(gameObject);
        }
        else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemies")) 
        {
            collision.collider.GetComponentInParent<EnemyBase>().DamageEnemy(normalDamage + Random.Range(-1, 1));
            Destroy(gameObject);
        } 
        else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) Destroy(gameObject);
    }
}
