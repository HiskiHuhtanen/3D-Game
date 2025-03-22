using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class AiRangedEnemy : MonoBehaviour
{
    public Transform player;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootingRange = 6f;
    public float projectileSpeed = 10f;
    public float attackCooldown = 2f;
    public float health = 1f;
    private Animator animator;
    public AudioClip deathSound;
    
    private AudioSource audioSource;
    private NavMeshAgent agent;
    private bool isAttacking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isAttacking)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            
            if (distanceToPlayer > shootingRange)
            {
                agent.isStopped = false;
                agent.destination = player.position;
                animator.SetFloat("Speed", agent.velocity.magnitude);
            }
            else
            {
                agent.isStopped = true;
                _ = StartAttackAsync();
            }
        }
    }

    async Task StartAttackAsync()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

            await Task.Delay(500);

            ShootProjectile();

            await Task.Delay((int)(attackCooldown * 500));
            isAttacking = false;
        }
    }

    void ShootProjectile()
    {
        if (projectilePrefab && firePoint)
        {
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
            Rigidbody rb = projectile.AddComponent<Rigidbody>();
            rb.useGravity = false;

            Vector3 direction = (player.position - firePoint.position).normalized;
            rb.linearVelocity = direction * projectileSpeed;

            Destroy(projectile, 3f);
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

    private void Die()
    {
        PlayDeathSound();
        Destroy(gameObject, deathSound ? deathSound.length : 0);
    }

    private void PlayDeathSound()
    {
        if (audioSource && deathSound)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }
}
