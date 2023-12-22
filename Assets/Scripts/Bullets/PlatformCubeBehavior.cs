using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformCubeBehavior : MonoBehaviour
{
    [SerializeField] float destroyDelay = 8f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(this.gameObject, destroyDelay);
    }
}
