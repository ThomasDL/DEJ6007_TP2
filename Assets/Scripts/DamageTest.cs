using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTest : MonoBehaviour
{
    [SerializeField] private float damageAmount;
    [SerializeField] private bool doDamageOverTime;
    [SerializeField] private float delayBetweenDamageTicks;

    private float timeElapsed;

    private void Start()
    {
        timeElapsed = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        timeElapsed = 0;

        if (other.CompareTag("Player"))
        {
            PlayerController_test.OnTakeDamage(damageAmount);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!doDamageOverTime) return;

        timeElapsed += Time.deltaTime;

        if (timeElapsed > delayBetweenDamageTicks)
        {
            PlayerController_test.OnTakeDamage(damageAmount);
            timeElapsed = 0;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        timeElapsed = 0;
    }
}
