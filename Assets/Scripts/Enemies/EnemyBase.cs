using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState { Patrolling, Attacking};
public abstract class EnemyBase : MonoBehaviour
{
    public float patrolSpeed;
    public float attackSpeed;
    protected EnemyState enemyState;

    protected NavMeshAgent navMeshAgent;
    public Transform[] patrolWaypoints;
    protected int currentPatrolWaypoint;

    protected GameObject player;
    public Transform visionPoint;
    public float visionDistance;

    public float attackDelay;
    protected float timeSinceLastAttack;

    protected void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");
        enemyState = EnemyState.Patrolling;
        navMeshAgent.SetDestination(patrolWaypoints[currentPatrolWaypoint].position);
    }
    protected bool CheckIfPlayerIsInSight()
    {
        if (Vector3.Angle(visionPoint.forward, player.transform.position - visionPoint.transform.position) > 90) return false;
        else
        {
            if (Physics.Raycast(visionPoint.position, player.transform.position - visionPoint.transform.position, visionDistance, LayerMask.GetMask("Player"))
                && !Physics.Raycast(visionPoint.position, player.transform.position - visionPoint.transform.position, Vector3.Distance(player.transform.position, visionPoint.transform.position), LayerMask.GetMask("Ground")))
            {
                return true;
            }
            else return false;
        }
    }
}
