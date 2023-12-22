using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotgunBulletBehavior : MonoBehaviour
{
    int collisionCounter;
    float maxLifeSpan = 0.3f;
    float lifeSpan = 0f;

    private PlayerController_test _playerController;

    private void Start()
    {
        maxLifeSpan = maxLifeSpan + Random.Range(0f, 0.3f);
        _playerController = PlayerController_test.Instance;
    }
    private void Update()
    {
        lifeSpan += Time.deltaTime;
        if (lifeSpan > maxLifeSpan ) Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground")) collisionCounter++;
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Enemies"))
        {
            _playerController.PlaySoundOnPlayer(3, 1.5f);
            collision.collider.GetComponentInParent<EnemyBase>().DamageEnemy(1);
        }
        if (collisionCounter == 1) Destroy(gameObject);
    }
}
