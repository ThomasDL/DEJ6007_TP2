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
    protected float proximity = 12f;

    protected void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");
        enemyState = EnemyState.Patrolling;
        navMeshAgent.SetDestination(patrolWaypoints[currentPatrolWaypoint].position);
    }
    protected void Update()
    {
        if (enemyState == EnemyState.Patrolling)
        {
            if (Vector3.Distance(patrolWaypoints[currentPatrolWaypoint].position, transform.position) < 0.2f)
            {
                currentPatrolWaypoint = (currentPatrolWaypoint + 1) % patrolWaypoints.Length;
                navMeshAgent.SetDestination(patrolWaypoints[currentPatrolWaypoint].position);
            }
        }
    }
    protected bool CheckIfPlayerIsInSight()
    {
        if (Mathf.Abs(Vector3.Angle(visionPoint.forward, player.transform.position - visionPoint.transform.position)) > 80
            && Vector3.Distance(player.transform.position, visionPoint.transform.position) > proximity) return false;
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
