using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatController : EnemyBase
{
    float attackModeRepeatRate;
    float attackDistance = 6f;
    float attackDelay = 1.2f;
    int attackCount;

    private void Start()
    {
        attackModeRepeatRate = Random.Range(0.45f, 0.55f);
        InvokeRepeating("AttackMode", 0f, attackModeRepeatRate);
    }
    private new void Update()
    {
        base.Update();
        if (enemyState == EnemyState.Attacking)
        { 
            if (Vector3.Distance(player.transform.position, transform.position) < attackDistance && Mathf.Abs(Vector3.Angle(visionPoint.forward, player.transform.position - visionPoint.transform.position)) < 40)
            {
                navMeshAgent.isStopped = true;
                if(timeSinceLastAttack > attackDelay)
                {
                    timeSinceLastAttack = 0;
                    Debug.Log(gameObject.name + " is attacking the player! " + attackCount);
                    attackCount++;
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
        else if (enemyState == EnemyState.Attacking)
        {
            enemyState = EnemyState.Patrolling;
            navMeshAgent.SetDestination(patrolWaypoints[currentPatrolWaypoint].position);
            navMeshAgent.speed = patrolSpeed;
        }
    }
    
}
