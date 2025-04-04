using System;
using System.Collections;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.AI;

public class BrazierAI : MonoBehaviour
{
    public Transform player;
    public float attackCooldown = 1.5f;
    public float health = 1f;
    public float rollSpeed = 10f;
    public float rollDistance = 5f;
    public float postRollWaitTime = 1f;
    public float wanderRadius = 5f;
    public float wanderCooldown = 3f;
    public float aggroRange  = 10f;

    public GameObject fire;
    public float fireGap;

    public AudioClip deathSound;

    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;

    private bool isAttacking = false;
    private bool isJumping = false;
    private bool hasAggro = false;
    private float wanderTimer = 0f;
    private Vector3 rollTarget;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (!hasAggro && distanceToPlayer <= aggroRange)
        {
            hasAggro = true;
        }


        if (hasAggro && !isAttacking && !isJumping)
        {
            agent.destination = player.position;

            if (agent.velocity.magnitude > 0.1f) 
            {
                animator.SetTrigger("Run");
            }
        }
        else if (!hasAggro && !isAttacking && !isJumping)
        {
            if (!agent.hasPath || agent.remainingDistance < 1f)
            {
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius, -1);
                agent.SetDestination(newPos);
            }

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
        rollDirection.y = 0; //ei lennä enää, hajoaa myöhemmin kyllä
        rollTarget = transform.position + rollDirection * rollDistance;

        Vector3 bounceTarget = (player.position - transform.position).normalized; //lol, otin vain näin ettei valita, tämä ei siis tarkoita mitää, en osannu vaa tehä sitä null:iksi :D
        Vector3 bounceDirection = (player.position - transform.position).normalized;
        bool hitObstacle = false;

        if (Physics.Raycast(transform.position, rollDirection, out RaycastHit hit, rollDistance))
        {
            if (!hit.collider.CompareTag("Enemy"))
            {
                float surfaceAngle = Vector3. Angle(Vector3.up, hit.normal);
                if (surfaceAngle > 30f)
                {
                    hitObstacle = true;
                    rollTarget = hit.point;
                    Vector3 normal = hit.normal;
                    bounceDirection = Vector3.Reflect(rollDirection, normal);
                }               
            }
        }

        animator.SetTrigger("Jump");

        yield return new WaitForSeconds(1f);

        agent.acceleration = 8f;

        animator.SetTrigger("Roll");

        Coroutine fireTrailRoutine = StartCoroutine(SpawnFireTrail());
        
        yield return MoveAtConstantSpeed(transform.position, rollTarget, rollSpeed);

        StopCoroutine(SpawnFireTrail());

        if (hitObstacle)
        {
            bounceTarget = rollTarget + bounceDirection * (rollDistance / 2f);
            yield return MoveAtConstantSpeed(rollTarget, bounceTarget, rollSpeed);
        }

        StopCoroutine(fireTrailRoutine);

        yield return new WaitForSeconds(postRollWaitTime);


        isJumping = false;
        isAttacking = false;
        agent.isStopped = false;
        agent.speed = 3.5f;
    }

    private IEnumerator MoveAtConstantSpeed(Vector3 start, Vector3 end, float speed)
    {
        float distance = Vector3.Distance(start, end);
        float travelTime = distance / speed;
        float elapsedTime = 0f;
        
        while (elapsedTime < travelTime)
        {
            transform.position = Vector3.Lerp(start, end, elapsedTime / travelTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        transform.position = end;
    }

    private IEnumerator SpawnFireTrail()
    {
        while (isJumping || isAttacking)
        {
            GameObject fireCopy = Instantiate(fire, transform.position - transform.forward * 0.5f, Quaternion.identity); //quaternionit pelottaa
            Destroy(fireCopy, 5);
            yield return new WaitForSeconds(fireGap);
        }
    }

    //tekoäly koodia
    public static Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
    {
        Vector3 randDirection = UnityEngine.Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
        return navHit.position;
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
