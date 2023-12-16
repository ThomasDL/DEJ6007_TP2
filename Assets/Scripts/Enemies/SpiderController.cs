using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderController : EnemyBase 
{
    public GameObject bulletPrefab;
    public Transform shootingPoint;

    Rigidbody thisRigidbody;

    float checkPlayerRepeatRate = 0.5f;
    float rotationSpeed = 80f;

    float timeSinceLastAttack;
    float nextAttackDelay;
    float attackRepeatMin = 0.5f;
    float attackRepeatMax = 0.9f;
    float attackForce = 15f;

    float timeSinceJumping;
    float nextJumpDelay;
    float jumpRepeatMin = 3f;
    float jumpRepeatMax = 5f;
    float jumpForce = 300f;
    float jumpPrecision = 8f;
    bool isGrounded = true;

    void Start()
    {
        thisAnim = GetComponentInChildren<Animator>();
        thisRigidbody = GetComponent<Rigidbody>();
        InvokeRepeating("SetEnemyState", 0f, checkPlayerRepeatRate);
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
    }

    new void Update()
    {
        base.Update();
        timeSinceLastAttack += Time.deltaTime;
        timeSinceJumping += Time.deltaTime;

        if (enemyState == EnemyState.Attacking && isAlive)
        {
            if (timeSinceLastAttack > nextAttackDelay)
            {
                timeSinceLastAttack = 0f;
                nextAttackDelay = Random.Range(attackRepeatMin, attackRepeatMax);
                thisAnim.SetTrigger("Attack");
                thisAudioSource.Play();
                Instantiate(bulletPrefab, shootingPoint.position, Quaternion.identity).GetComponent<Rigidbody>().AddForce(Vector3.Normalize(player.transform.position - visionPoint.position) * attackForce, ForceMode.Impulse);
            }
        }
        SetWalkAnimationSpeed();
    }
    void SetWalkAnimationSpeed()
    {
        previousPosition = currentPosition;
        currentPosition = transform.position;
        float speed = Vector3.Distance(currentPosition, previousPosition) / Time.deltaTime;
        thisAnim.SetFloat("Speed", speed);
    }
    private void FixedUpdate()
    {
        if (enemyState == EnemyState.Attacking && isAlive)
        {
            RotateBodyTowardsPlayer();

            if (timeSinceJumping > nextJumpDelay && isGrounded)
            {
                timeSinceJumping = 0f;
                nextJumpDelay = Random.Range(jumpRepeatMin, jumpRepeatMax);
                thisRigidbody.AddForce(Vector3.up * jumpForce + new Vector3(Random.Range(-jumpPrecision, jumpPrecision), Random.Range(-1, 1), Random.Range(-jumpPrecision, jumpPrecision)), ForceMode.Impulse);
                isGrounded = false;
                thisAnim.SetTrigger("Jump");
            }
        }
    }
    void RotateBodyTowardsPlayer()
    {
        // Determine the direction to the player
        Vector3 directionToPlayer = player.transform.position - transform.position;

        directionToPlayer.y = 0;

        // Create a rotation that points in that direction
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);

        // Calculate the rotation step based on the speed and frame rate
        float step = rotationSpeed * Time.deltaTime;

        // Rotate towards the target rotation
        thisRigidbody.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
    }
    void SetEnemyState()
    {
        if (isAlive)
        {
            if (base.CheckIfPlayerIsInSight() && enemyState == EnemyState.Patrolling)
            {
                navMeshAgent.enabled = false;
                thisRigidbody.isKinematic = false;
                enemyState = EnemyState.Attacking;
            }
            else if (isGrounded && enemyState == EnemyState.Attacking && !base.CheckIfPlayerIsInSight())
            {
                thisRigidbody.isKinematic = true;
                navMeshAgent.enabled = true;
                enemyState = EnemyState.Patrolling;
                navMeshAgent.SetDestination(patrolWaypoints[currentPatrolWaypoint].position);
            }
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")) isGrounded = true;
    }
}
