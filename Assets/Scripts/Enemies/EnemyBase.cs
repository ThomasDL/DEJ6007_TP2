using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public enum EnemyState { Patrolling, Attacking};
public abstract class EnemyBase : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth {  get; protected set; }

    protected bool isAlive = true;

    public float patrolSpeed;
    protected EnemyState enemyState;

    public Slider healthSlider;
    protected NavMeshAgent navMeshAgent;
    public Transform[] patrolWaypoints;
    protected int currentPatrolWaypoint;

    protected Animator thisAnim;

    protected GameObject player;
    public Transform visionPoint;
    public float visionDistance;
    protected float proximity = 12f;

    protected float timeSincePlayerWasSeen = 5f;
    protected const float timeBeforePlayerIsForgotten = 3f;

    protected Vector3 previousPosition;
    protected Vector3 currentPosition;

    protected void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        currentHealth = maxHealth;
        player = GameObject.Find("Player");
        enemyState = EnemyState.Patrolling;
        navMeshAgent.SetDestination(patrolWaypoints[currentPatrolWaypoint].position);
    }
    protected void Update()
    {
        timeSincePlayerWasSeen += Time.deltaTime;
        if (enemyState == EnemyState.Patrolling && isAlive)
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
            && timeSincePlayerWasSeen > timeBeforePlayerIsForgotten) return false;
        else
        {
            if (Physics.Raycast(visionPoint.position, player.transform.position - visionPoint.transform.position, visionDistance, LayerMask.GetMask("Player"))
                && !Physics.Raycast(visionPoint.position, player.transform.position - visionPoint.transform.position, Vector3.Distance(player.transform.position, visionPoint.transform.position), LayerMask.GetMask("Ground")))
            {
                timeSincePlayerWasSeen = 0;
                return true;
            }
            else if (timeSincePlayerWasSeen < timeBeforePlayerIsForgotten) return true;
            else return false;
        }
    }
    public void DamageEnemy(float damage)
    {
        enemyState = EnemyState.Attacking;
        timeSincePlayerWasSeen = 0f;
        currentHealth -= damage;
        healthSlider.gameObject.SetActive(true);
        healthSlider.value = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (currentHealth <= 0 && isAlive)
        {
            StartCoroutine(EnemyDies());
        }
    }
    protected IEnumerator EnemyDies()
    {
        isAlive = false;
        navMeshAgent.enabled = false;
        thisAnim.SetTrigger("Dead");
        float timeToDie = 1f;
        float totalTime = 0f;
        while (totalTime < timeToDie)
        {
            totalTime += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
