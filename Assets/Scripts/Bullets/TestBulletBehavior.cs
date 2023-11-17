using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBulletBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AutoDestroy());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator AutoDestroy()
    {
        yield return new WaitForSeconds(3f);
        Destroy(gameObject);
    }
}
