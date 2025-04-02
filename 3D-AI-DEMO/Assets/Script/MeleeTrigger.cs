using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

public class MeleeTrigger : MonoBehaviour
{
    public Transform player;
    public float attackCooldown = 1.5f;
    public GameObject slashEffect;
    public Transform attackBox;
    public float health = 1f;
    public AudioClip deathSound;
    private AudioSource audioSource;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isAttacking = false;

    public Material dissolveMaterial;
    public float dissolveSpeed = 1f;
    private float dissolveAmount = -1f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        dissolveMaterial = GetComponentInChildren<SkinnedMeshRenderer>().material;
    }

    void Update()
    {
        if (!isAttacking)
        {
            agent.destination = player.position;
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PLAYER"))
        {
            if (!isAttacking)
            {
                _ = StartAttackAsync();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log(gameObject.name + " left attack range!");
        }
    }

    async Task StartAttackAsync()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            agent.isStopped = true;
            animator.SetFloat("Speed", 0);

            Vector3 direction = (player.position - transform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = lookRotation;

            await Task.Delay(250);

            slashEffect.GetComponent<ParticleSystem>().Play();

            //tarkistetaan osuuko pelaajaan
            Collider[] hitObjects = Physics.OverlapBox(attackBox.position, attackBox.localScale, attackBox.rotation);
            foreach (Collider hit in hitObjects)
            {   
                if (hit.CompareTag("PLAYER"))
                {
                    hit.GetComponent<Player>().TakeDamage(1f);
                    Debug.Log("Player hit by " + gameObject.name);
                }
            }

            Invoke(nameof(ResetAttack), attackCooldown);
        }
    }

    void ResetAttack()
    {
        isAttacking = false;
        agent.isStopped = false;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0) {
            Die();
        }
    }

    public void Die()
    {   
        PlayDeathSound();
        StartCoroutine(DissolveEffect());
    }

    IEnumerator DissolveEffect()
    {
        while (dissolveAmount < 1f)
        {
            dissolveAmount += Time.deltaTime * dissolveSpeed;
            Debug.Log("Dissolve Value: " + dissolveAmount);
            dissolveMaterial.SetFloat("_DissolveAmount", dissolveAmount);
            Debug.Log("Dissolve Value: " + dissolveAmount);
            yield return null;
        }

        Destroy(gameObject);

    }

    private void PlayDeathSound()
    {
        if (audioSource && deathSound)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }
}