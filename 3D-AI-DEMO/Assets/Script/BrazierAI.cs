using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BrazierAI : MonoBehaviour
{
    public Transform player;
    public float attackCooldown = 1.5f;
    public float health = 1f;
    public float rollSpeed = 10f;
    public float rollDistance = 5f;
    public float rollDuration = 1.5f;
    public float postRollWaitTime = 1f;

    public GameObject fire;
    public float fireGap;

    public AudioClip deathSound;

    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;

    private bool isAttacking = false;
    private bool isJumping = false;
    private Vector3 rollTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!isAttacking && !isJumping)
        {
            agent.destination = player.position;

            if (agent.velocity.magnitude > 0.1f) 
            {
                animator.SetTrigger("Run");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PLAYER") && !isAttacking && !isJumping)
        {
            StartCoroutine(JumpAndRollAttack());
        }
    }

    private IEnumerator JumpAndRollAttack()
    {
        isJumping = true;
        isAttacking = true;
        agent.isStopped = true;
        agent.velocity = Vector3.zero; //nämä pois jos haluaa "lunge"
        agent.acceleration = 0; //tämäkin siis

        Vector3 rollDirection = (player.position - transform.position).normalized;
        rollTarget = transform.position + rollDirection * rollDistance;
        animator.SetTrigger("Jump");

        yield return new WaitForSeconds(1f);

        agent.acceleration = 8f;

        animator.SetTrigger("Roll");

        float elapsedTime = 0f;
        Vector3 startPosition = transform.position;

        StartCoroutine(SpawnFireTrail());

        // Move towards rollTarget over rollDuration time
        while (elapsedTime < rollDuration)
        {
            transform.position = Vector3.Lerp(startPosition, rollTarget, elapsedTime / rollDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = rollTarget;
        yield return new WaitForSeconds(postRollWaitTime);


        isJumping = false;
        isAttacking = false;
        agent.isStopped = false;
        agent.speed = 3.5f;
    }

    private IEnumerator SpawnFireTrail()
    {
        while (isJumping || isAttacking)
        {
            Instantiate(fire, transform.position - transform.forward * 0.5f, Quaternion.identity); //quaternionit pelottaa
            yield return new WaitForSeconds(fireGap);
        }
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        PlayDeathSound();
        Destroy(gameObject, deathSound.length);
    }

    private void PlayDeathSound()
    {
        if (audioSource && deathSound)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }
}
