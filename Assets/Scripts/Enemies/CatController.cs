using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController : EnemyBase
{
    float attackModeRepeatRate = 0.45f;
    float attackDistance = 6f;
    float attackDelay = 1.2f;
    int attackCount;
    float timeSinceLastAttack;

    Animator catAnim;

    private void Start()
    {
        catAnim = GetComponentInChildren<Animator>();
        InvokeRepeating("DecisionCheck", 0f, attackModeRepeatRate);
    }
    private new void Update()
    {
        base.Update();
        timeSinceLastAttack += Time.deltaTime;
        if (enemyState == EnemyState.Attacking)
        { 
            if (Vector3.Distance(player.transform.position, transform.position) < attackDistance && Mathf.Abs(Vector3.Angle(visionPoint.forward, player.transform.position - visionPoint.transform.position)) < 40)
            {
                navMeshAgent.isStopped = true;
                if(timeSinceLastAttack > attackDelay)
                {
                    timeSinceLastAttack = 0;
                    catAnim.SetTrigger("Attack");
                    Debug.Log("Cat has attacked " + attackCount);
                    attackCount++;
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
        catAnim.SetFloat("Speed", speed);
    }
    void DecisionCheck()
    {
        if (base.CheckIfPlayerIsInSight())
        {
            enemyState = EnemyState.Attacking;
            navMeshAgent.speed = attackSpeed;
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
