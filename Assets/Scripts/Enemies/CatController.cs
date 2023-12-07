using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController : EnemyBase
{
    public float runSpeed;
    public bool isMamaCat;
    public float attackStrength;

    float attackModeRepeatRate = 0.45f;
    float attackDistance = 6f;
    float mamaAttackDistance = 13f;
    float attackDelay = 1.2f;
    float timeSinceLastAttack;

    private void Start()
    {
        thisAnim = GetComponentInChildren<Animator>();
        InvokeRepeating("DecisionCheck", 0f, attackModeRepeatRate);
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
        if (isMamaCat) attackDistance = mamaAttackDistance;
    }
    private new void Update()
    {
        base.Update();
        timeSinceLastAttack += Time.deltaTime;
        if (enemyState == EnemyState.Attacking && isAlive)
        { 
            if (Vector3.Distance(player.transform.position, transform.position) < attackDistance && Mathf.Abs(Vector3.Angle(visionPoint.forward, player.transform.position - visionPoint.transform.position)) < 35)
            {
                navMeshAgent.isStopped = true;
                if(timeSinceLastAttack > attackDelay)
                {
                    timeSinceLastAttack = 0;
                    thisAnim.SetTrigger("Attack");
                    PlayerController_test.OnTakeDamage(attackStrength);
                }
            }
            else navMeshAgent.isStopped = false;
        }
        HandleWalk();
    }
    void HandleWalk()
    {
        previousPosition = currentPosition;
        currentPosition = transform.position;
        float speed = Vector3.Distance(currentPosition, previousPosition) / Time.deltaTime;
        thisAnim.SetFloat("Speed", speed * (isMamaCat? 0.5f : 1f));
    }
    void DecisionCheck()
    {
        if (isAlive)
        {
            if (base.CheckIfPlayerIsInSight())
            {
                enemyState = EnemyState.Attacking;
                navMeshAgent.speed = runSpeed;
                navMeshAgent.SetDestination(player.transform.position);
            }
            else if (enemyState == EnemyState.Attacking)
            {
                enemyState = EnemyState.Patrolling;
                navMeshAgent.SetDestination(patrolWaypoints[currentPatrolWaypoint].position);
                navMeshAgent.speed = patrolSpeed;
            }
        }
    }
}
