using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BullfrogGrenadeBehavior : MonoBehaviour
{
    Transform playerTransform;
    public ParticleSystem explosionParticles;

    float explosionOnPlayerStrength = 25f;
    float explosionOnEnemyStrength = 10f;
    float explosionRadius = 7f;
    
    private void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
    }

    private void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(Explosion());
    }
    IEnumerator Explosion()
    {
        explosionParticles.Play();
        GetComponent<MeshRenderer>().enabled = false;
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        if(colliders.Length > 0)
        {
            foreach(Collider collider in colliders)
            {
                if (collider.CompareTag("Player")) PlayerController_test.OnTakeDamage(explosionOnPlayerStrength);
                else if (collider.TryGetComponent<EnemyBase>(out EnemyBase enemyBase)) enemyBase.DamageEnemy(explosionOnEnemyStrength);
            }
        }
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
