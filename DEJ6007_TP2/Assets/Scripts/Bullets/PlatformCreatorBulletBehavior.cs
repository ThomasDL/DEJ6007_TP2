using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCreatorBulletBehavior : MonoBehaviour
{
    public GameObject platformCubePrefab;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Instantiate(platformCubePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
