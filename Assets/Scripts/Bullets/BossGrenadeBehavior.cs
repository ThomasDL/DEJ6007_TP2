using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGrenadeBehavior : MonoBehaviour
{
    Transform playerTransform;
    public ParticleSystem explosionParticles;
    
    int bounceCount;
    float explosionRadius = 7f;
    
    private void Start()
    {
        playerTransform = GameObject.Find("Player").transform;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(Explosion());
            
        } else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            if (Vector3.Distance(playerTransform.position, transform.position) < explosionRadius)
            {
                StartCoroutine(Explosion());
            } else
            {
                bounceCount++;
                if (bounceCount == 2)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
    IEnumerator Explosion()
    {
        Debug.Log("The boss has hit the player with a grenade!");
        explosionParticles.Play();
        GetComponent<MeshRenderer>().enabled = false;
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
