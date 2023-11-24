using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SquirrelController : EnemyBase 
{
    public GameObject bulletPrefab;
    public Transform shootingPoint;

    Rigidbody thisRigidbody;

    float checkPlayerRepeatRate = 0.5f;
    float turnDelay = 0.4f;

    float timeSinceLastAttack;
    float nextAttackDelay;
    float attackRepeatMin = 0.6f;
    float attackRepeatMax = 1f;
    float attackForce = 10f;

    float timeSinceJumping;
    float nextJumpDelay;
    float jumpRepeatMin = 3f;
    float jumpRepeatMax = 5f;
    float jumpForce = 15f;
    float jumpPrecision = 8f;
    bool isGrounded = true;


    void Start()
    {
        thisRigidbody = GetComponent<Rigidbody>();
        InvokeRepeating("SetEnemyState", 0f, checkPlayerRepeatRate);
    }

    new void Update()
    {
        base.Update();
        timeSinceLastAttack += Time.deltaTime;
        timeSinceJumping += Time.deltaTime;

        if (enemyState == EnemyState.Attacking)
        {
            if (timeSinceLastAttack > nextAttackDelay)
            {
                timeSinceLastAttack = 0f;
                nextAttackDelay = Random.Range(attackRepeatMin, attackRepeatMax);
                Instantiate(bulletPrefab, shootingPoint.position, Quaternion.identity).GetComponent<Rigidbody>().AddForce(Vector3.Normalize(player.transform.position - visionPoint.position) * attackForce, ForceMode.Impulse);
            }
        }
    }
    private void FixedUpdate()
    {
        if (enemyState == EnemyState.Attacking)
        {
            Vector3 direction = player.transform.position - transform.position;
            direction.y = 0f;
            var rotation = Quaternion.LookRotation(direction);
            thisRigidbody.MoveRotation(rotation);

            if (timeSinceJumping > nextJumpDelay && isGrounded)
            {
                timeSinceJumping = 0f;
                nextJumpDelay = Random.Range(jumpRepeatMin, jumpRepeatMax);
                thisRigidbody.AddForce(Vector3.up * jumpForce + new Vector3(Random.Range(-jumpPrecision, jumpPrecision), Random.Range(-1, 1), Random.Range(-jumpPrecision, jumpPrecision)), ForceMode.Impulse);
                isGrounded = false;
            }
        }
    }
    void SetEnemyState()
    {
        if (base.CheckIfPlayerIsInSight() && enemyState == EnemyState.Patrolling) 
        {
            StartCoroutine(TurnTowardPlayer());
        }
        else if (isGrounded && enemyState == EnemyState.Attacking && !base.CheckIfPlayerIsInSight())
        {
            thisRigidbody.isKinematic = true;
            navMeshAgent.enabled = true;
            enemyState = EnemyState.Patrolling;
            navMeshAgent.SetDestination(patrolWaypoints[currentPatrolWaypoint].position);
        } 
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")) isGrounded = true;
    }
    IEnumerator TurnTowardPlayer()
    {
        navMeshAgent.SetDestination(player.transform.position);
        yield return new WaitForSeconds(turnDelay);
        navMeshAgent.enabled = false;
        thisRigidbody.isKinematic = false;
        enemyState = EnemyState.Attacking;
    }
}
