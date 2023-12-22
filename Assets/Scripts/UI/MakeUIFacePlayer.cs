using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeUIFacePlayer : MonoBehaviour
{
    Transform player;
    // Start is called before the first frame update
    void Start()
    {
     player = GameObject.Find("Player_test_achraf").GetComponent<Transform>();   
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(player.position);
    }
}
