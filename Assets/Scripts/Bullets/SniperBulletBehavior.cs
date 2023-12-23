using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SniperBulletBehavior : MonoBehaviour
{
    int headshotDamage = 14;
    int normalDamage = 7;


    private PlayerController_test _playerController;

    private void Start()
    {
        _playerController = PlayerController_test.Instance;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Head"))
        {
            collision.collider.GetComponentInParent<EnemyBase>().DamageEnemy(headshotDamage + Random.Range(-1,1));
            Destroy(gameObject);
        }
        else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            _playerController.PlaySoundOnPlayer(3, 2f);
            Debug.Log("Sniper hit !");
            collision.collider.GetComponentInParent<EnemyBase>().DamageEnemy(normalDamage + Random.Range(-1, 1));
            Destroy(gameObject);
        } 
        else if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) Destroy(gameObject);
    }
}
