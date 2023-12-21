using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BullfrogGrenadeBehavior : MonoBehaviour
{
    Transform playerTransform;
    public ParticleSystem explosionParticles;
    AudioSource grenadeAudioSource;

    float explosionOnPlayerStrength = 25f;
    float explosionOnEnemyStrength = 8f;
    float explosionRadius = 7f;
    
    private void Start()
    {
        playerTransform = GameObject.Find("Player_test_achraf").transform;
        grenadeAudioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        StartCoroutine(Explosion());
    }
    IEnumerator Explosion()
    {
        explosionParticles.Play();
        grenadeAudioSource.Play();
        GetComponent<MeshRenderer>().enabled = false;
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        if(colliders.Length > 0)
        {
            foreach(Collider collider in colliders)
            {
                if (collider.CompareTag("Player")) PlayerController_test.OnTakeDamage(explosionOnPlayerStrength + Random.Range(-5,5));
                else if (collider.TryGetComponent<EnemyBase>(out EnemyBase enemyBase)) enemyBase.DamageEnemy(explosionOnEnemyStrength + Random.Range(-3,3));
            }
        }
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
