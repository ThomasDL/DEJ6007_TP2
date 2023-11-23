using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController : EnemyBase
{
    float attackModeRepeatRate;
    float attackDistance = 8f;

    private void Start()
    {
        attackModeRepeatRate = Random.Range(0.45f, 0.55f);
        InvokeRepeating("AttackMode", 0f, attackModeRepeatRate);
    }
    private void Update()
    {
        timeSinceLastAttack += Time.deltaTime;
        if (enemyState == EnemyState.Patrolling)
        {
            if(Vector3.Distance(patrolWaypoints[currentPatrolWaypoint].position, transform.position) < 0.2f)
            {
                currentPatrolWaypoint = (currentPatrolWaypoint + 1) % patrolWaypoints.Length;
                navMeshAgent.SetDestination(patrolWaypoints[currentPatrolWaypoint].position);
            }
        }
        if (enemyState == EnemyState.Attacking)
        { 
            if (Vector3.Distance(player.transform.position, transform.position) < attackDistance)
            {
                navMeshAgent.isStopped = true;
                if(timeSinceLastAttack > attackDelay)
                {
                    timeSinceLastAttack = 0;
                    Debug.Log(gameObject.name + " is attacking the player!");
                }
            }
            else navMeshAgent.isStopped = false;
        }
    }
    void AttackMode()
    {
        if (base.CheckIfPlayerIsInSight())
        {
            enemyState = EnemyState.Attacking;
            navMeshAgent.speed = attackSpeed;
            navMeshAgent.SetDestination(player.transform.position);
        }
        else
        {
            enemyState = EnemyState.Patrolling;
            navMeshAgent.SetDestination(patrolWaypoints[currentPatrolWaypoint].position);
            navMeshAgent.speed = patrolSpeed;
        }
    }
    
}
