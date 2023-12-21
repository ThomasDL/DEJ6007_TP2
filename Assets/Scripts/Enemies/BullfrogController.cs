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
    float strafeJumpForce = 8000f;
    float grenadeWaitTime = 0.7f;
    float grenadeThrowForce = 9.2f;

    float jumpStrength = 30f;
    float jumpForce = 2210500f;
    float jumpWaitTime = 1f;
    bool isJumping = false;
    float jumpImpactRadius = 12f;

    void Start()
    {
        bossRigidbody = GetComponent<Rigidbody>();
        InvokeRepeating("VisionCheck", visionCheckRate, visionCheckRate);
        healthSlider.maxValue = maxHealth;
        healthSlider.value = maxHealth;
    }
    new void Update()
    {
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
        if (enemyState == EnemyState.Attacking && isAlive) RotateBodyTowardsPlayer();
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
        // The frog jumps towards the player
        bossRigidbody.velocity = Vector3.zero;
        isAttacking = true;
        float jumpTotal = Mathf.Sqrt(Vector3.Distance(player.transform.position, transform.position) * jumpForce);
        bossRigidbody.AddForce(Vector3.Normalize(player.transform.position - transform.position) * jumpTotal + transform.up * jumpTotal, ForceMode.Impulse);
        isJumping = true;
        thisAudioSource.Play();
        thisAnim.SetFloat("JumpLength", 4000 / jumpTotal);
        thisAnim.SetTrigger("Jump");
    }
    IEnumerator GrenadeAttack()
    {
        // First, the frog jumps to the side
        isAttacking = true;
        bossRigidbody.AddForce(transform.right * strafeJumpForce * Random.Range(0.7f, 1f) * (Random.Range(0,2) == 0 ? -1: 1) + transform.up * strafeJumpForce * 0.5f, ForceMode.Impulse);
        yield return new WaitForSeconds(grenadeWaitTime);

        // Then, it throws a grenade at the player
        if (isAlive)
        {
            thisAudioSource.Play();
            thisAnim.SetTrigger("Tongue");
            Rigidbody grenadeRb = Instantiate(grenadePrefab, grenadeThrowPoint.position, Quaternion.identity).GetComponent<Rigidbody>();
            grenadeRb.AddForce(Vector3.Normalize(player.transform.position - grenadeThrowPoint.position) * Mathf.Sqrt(Vector3.Distance(player.transform.position, grenadeThrowPoint.position) * grenadeThrowForce) + transform.up * Mathf.Sqrt(Vector3.Distance(player.transform.position, grenadeThrowPoint.position) * grenadeThrowForce), ForceMode.Impulse);
            yield return new WaitForSeconds(grenadeWaitTime);
            isAttacking = false;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        // If the frog hits the ground after jumping, it creates a shockwave that can hurt the player
        if (isJumping && (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.CompareTag("Player")) && isAlive)
        {
            isJumping = false;
            jumpForceParticles.Play();
            jumpForceParticles.gameObject.GetComponent<AudioSource>().Play();
            thisAnim.SetTrigger("Idle");
            if (Vector3.Distance(player.transform.position, transform.position) < jumpImpactRadius)
            {
                PlayerController_test.OnTakeDamage(jumpStrength + Random.Range(-5, 5));
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
