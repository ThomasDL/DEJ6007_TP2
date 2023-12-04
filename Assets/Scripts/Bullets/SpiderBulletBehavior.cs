using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderBulletBehavior : MonoBehaviour
{
    float timeToDestroy = 10f;
    float timeSpent = 0f;

    private void Update()
    {
        timeSpent += Time.deltaTime;
        if (timeSpent > timeToDestroy) Destroy(gameObject);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController_test.OnTakeDamage(10);
            Destroy(gameObject);
        }
    }
}
