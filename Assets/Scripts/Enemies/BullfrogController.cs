using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BullfrogController : EnemyBase
{
    Rigidbody bossRigidbody;
    public ParticleSystem jumpForceParticles;
    public GameObject grenadePrefab;

    float visionCheckRate = 0.61f;
    float rotationSpeed = 50f;

    bool isAttacking = false;

    public Transform grenadeThrowPoint;
    float strafeJumpForce = 3000f;
    float grenadeWaitTime = 0.6f;
    float grenadeThrowForce = 4.6f;

    float jumpStrength = 30f;
    float jumpForce = 1105000f;
    float jumpWaitTime = 1f;
    bool isJumping = false;
    float jumpImpactRadius = 12f;

    void Start()
    {
        thisAnim = GetComponentInChildren<Animator>();
        bossRigidbody = GetComponent<Rigidbody>();
        InvokeRepeating("VisionCheck", visionCheckRate, visionCheckRate);
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
    }
    new void Update()
    {
        HandleWalk(); 
    }
    void HandleWalk()
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
            RotateTowardsPlayer();
        }
    }
    void InitiateRandomAttack()
    {
        switch (Random.Range(0, 2))
        {
            case 0:
                JumpingAttack();
                break;
            case 1:
                StartCoroutine(GrenadeAttack());
                break;
        }
    }
    void RotateTowardsPlayer()
    {
        // Determine the direction to the player
        Vector3 directionToPlayer = player.transform.position - transform.position;

        directionToPlayer.y = 0;

        // Create a rotation that points in that direction
        Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);

        // Calculate the rotation step based on the speed and frame rate
        float step = rotationSpeed * Time.deltaTime;

        // Rotate towards the target rotation
        bossRigidbody.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, step);
    }
    void VisionCheck()
    {
        if (isAlive)
        {
            if (base.CheckIfPlayerIsInSight() && !isAttacking)
            {
                enemyState = EnemyState.Attacking;
                navMeshAgent.enabled = false;
                bossRigidbody.isKinematic = false;
                InitiateRandomAttack();

            }
            else if (!base.CheckIfPlayerIsInSight() && enemyState == EnemyState.Attacking && !isAttacking)
            {
                enemyState = EnemyState.Patrolling;
                bossRigidbody.isKinematic = true;
                navMeshAgent.enabled = true;
                navMeshAgent.SetDestination(player.transform.position);
            }
        }
    }
    void JumpingAttack()
    {
        bossRigidbody.velocity = Vector3.zero;
        isAttacking = true;
        float jumpTotal = Mathf.Sqrt(Vector3.Distance(player.transform.position, transform.position) * jumpForce);
        bossRigidbody.AddForce(Vector3.Normalize(player.transform.position - transform.position) * jumpTotal + transform.up * jumpTotal, ForceMode.Impulse);
        isJumping = true;
        thisAnim.SetFloat("JumpLength", 4000 / jumpTotal);
        thisAnim.SetTrigger("Jump");
    }
    IEnumerator GrenadeAttack()
    {
        isAttacking = true;
        bossRigidbody.AddForce(transform.right * strafeJumpForce * Random.Range(-1f, 1f) + transform.up * strafeJumpForce, ForceMode.Impulse);
        yield return new WaitForSeconds(grenadeWaitTime);
        if (isAlive)
        {
            thisAnim.SetTrigger("Tongue");
            Rigidbody grenadeRb = Instantiate(grenadePrefab, grenadeThrowPoint.position, Quaternion.identity).GetComponent<Rigidbody>();
            grenadeRb.AddForce(Vector3.Normalize(player.transform.position - grenadeThrowPoint.position) * Mathf.Sqrt(Vector3.Distance(player.transform.position, grenadeThrowPoint.position) * grenadeThrowForce) + transform.up * Mathf.Sqrt(Vector3.Distance(player.transform.position, grenadeThrowPoint.position) * grenadeThrowForce), ForceMode.Impulse);
            yield return new WaitForSeconds(grenadeWaitTime);
            isAttacking = false;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (isJumping && (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.CompareTag("Player")) && isAlive)
        {
            isJumping = false;
            jumpForceParticles.Play();
            thisAnim.SetTrigger("Idle");
            if (Vector3.Distance(player.transform.position, transform.position) < jumpImpactRadius)
            {
                PlayerController_test.OnTakeDamage(jumpStrength);
            }
            StartCoroutine(WaitAfterJump());
        }
    }
    IEnumerator WaitAfterJump()
    {
        yield return new WaitForSeconds(jumpWaitTime);
        isAttacking = false;
    }
}
