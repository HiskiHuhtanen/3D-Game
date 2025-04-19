using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


//sisältää PALJON turhia rivejä, oletan, en ole tarkistanut
//muutenkin nykyinen toiminta ja rakenne on hieman outo.

public class EliteAI : MonoBehaviour, IDamageable
{
    public Transform player;
    public GameObject fire;
    public GameObject warningLine;
    public GameObject explosion;
    public AudioClip deathSound;
    public float health = 1f;
    public AudioClip idleLoopSound;
    public AudioClip stepSound1;
    public AudioClip stepSound2;
    public AudioClip punchSound;
    public AudioClip fireAttackSound;
    public AudioClip spinAttackSound;
    public AudioSource idleLoopSource;
    public AudioSource sfxSource;

    private int rangeCooldown = 0;
    private int rangedAttackCounter = 0;
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private bool dead = false;
    private int step = 0;
    public float attackRange = 4f;   
    public float rangedAttackRange = 20f; 
    private bool isAttacking = false;
    public bool canPunch = true; //muuta nimi jossain vaiheessa globaaliksi attackCooldowniksi

    private DissolveController[] dissolveControllers;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (idleLoopSource && idleLoopSound)
        {
            idleLoopSource.clip = idleLoopSound;
            idleLoopSource.loop = true;
            idleLoopSource.Play();
        }

        dissolveControllers = GetComponentsInChildren<DissolveController>();
        if (dissolveControllers.Length == 0)
        {
            Debug.LogError("No DissolveController components found on the EliteAI or its children.");
        }
    }

    //ei toimi, tiedän miksi, en jaksanut korjata
    //kello on yö.

    //hahaaa! ei enää!
    void Update()
    {
        if (isAttacking || dead) return; 

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        animator.SetBool("isRunning", true);

        agent.SetDestination(player.position);

        if (distanceToPlayer <= attackRange && canPunch) 
        {
            StartCoroutine(Punch());
        }
        else if (distanceToPlayer <= rangedAttackRange && canPunch) 
        {
            if (distanceToPlayer <= rangedAttackRange && rangeCooldown == 0) 
            {
                if (rangedAttackCounter < 1)
                {
                    StartCoroutine(FireAttack());
                }
                else
                {
                    StartCoroutine(SpinAttack());
                    rangedAttackCounter = 0;
                }
            }
        }
    }

    private IEnumerator Punch()
    {
    if (dead) yield break;
    isAttacking = true;
    canPunch = false;

    animator.SetBool("isDashing", true);
    PlaySound(punchSound);
    yield return new WaitForSeconds(0.3f);

    Vector3 firstDashDirection = (transform.forward + transform.right).normalized;
    Vector3 firstDashPosition = transform.position + firstDashDirection * 6f; 
    yield return MoveToward(firstDashPosition, 20f);

    Vector3 directionToPlayer = (player.transform.position - firstDashPosition).normalized;
    Vector3 secondDashPosition = player.transform.position + directionToPlayer * -2f;
    //transform.position = secondDashPosition;
    //yield return new WaitForSeconds(0.5f);
    yield return MoveToward(secondDashPosition, 25f);

    animator.SetBool("isDashing", false);
    animator.SetBool("isPunching", true);
    agent.isStopped = true; 
    yield return new WaitForSeconds(1.5f);

    animator.SetBool("isPunching", false);

    agent.isStopped = false;
    isAttacking = false;
    //miksi näin???? eikä vaa rangeCooldown -= 1
    rangeCooldown = Mathf.Max(rangeCooldown - 1, 0);
    yield return new WaitForSeconds(4f);
    canPunch = true;
    }

    private IEnumerator MoveToward(Vector3 targetPos, float speed)
    {
        Vector3 start = transform.position;
        float distance = Vector3.Distance(start, targetPos);
        float travelTime = distance / speed;
        float elapsedTime = 0f;

        while (elapsedTime < travelTime)
        {
            transform.position = Vector3.Lerp(start, targetPos, elapsedTime / travelTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos;
    }

    private IEnumerator FireAttack()
    {
        if (dead) yield break;
        canPunch = false;
        isAttacking = true;
        agent.isStopped = true;
        animator.SetBool("isRanged", true);
        PlaySound(fireAttackSound);
        int fireCount = 10;
        float fireDistance = 1.5f;
        rangeCooldown = rangeCooldown + 2;
        
        for (int i = 0; i < fireCount; i++)
        {
            Vector3 fireSpawnPos = transform.position + transform.forward * (i * fireDistance + 1.5f);
            GameObject fireCopy = Instantiate(fire, fireSpawnPos, Quaternion.identity);
            Destroy(fireCopy, 5);
            yield return new WaitForSeconds(0.2f);  
        }
        animator.SetBool("isRanged", false);
        yield return new WaitForSeconds(1f);
        agent.isStopped = false;
        isAttacking = false;
        rangedAttackCounter += 1;
        yield return new WaitForSeconds(3f);
        canPunch = true;

    }

    //HOX HOX! Gpt auttoi tässä matikassa
    private IEnumerator SpinAttack()
    {
        if (dead) yield break;
        canPunch = false;
        isAttacking = true;
        agent.isStopped = true;
        Debug.Log("SPIIIIN");

        animator.SetTrigger("SpinStart");
        yield return new WaitForSeconds(0.8f);

        animator.SetBool("isSpinning" , true);

        //näitä voisi ehkä siirtää publiciksi
        int pairs = 8;
        float lineDistance = 5f;
        float rotationSpeed = 45f;
        GameObject[] lines = new GameObject[pairs * 2];
        float angleStep = 360f / pairs;
        float currentRotation = 0f;
        int attackRepeats = 3;
        int fireSpawnCount = 10;
        float minSpinTime = 1.5f;
        float maxSpinTime = 5f;
        PlaySound(spinAttackSound);

        for (int i = 0; i < pairs; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 offset = Vector3.Cross(dir, Vector3.up) * 1f;
            Vector3 pos1 = transform.position + dir * lineDistance + offset;
            Vector3 pos2 = transform.position + dir * lineDistance - offset;

            // Rotate 90 degrees around the Y-axis so the lines stand upright
            lines[i * 2] = Instantiate(warningLine, pos1, Quaternion.LookRotation(dir) * Quaternion.Euler(90, 0, 0));
            lines[i * 2 + 1] = Instantiate(warningLine, pos2, Quaternion.LookRotation(dir) * Quaternion.Euler(90, 0, 0));

        }
        for (int repeat = 0; repeat < attackRepeats; repeat++)
        {
            float timer = 0f;
            float spinTime = UnityEngine.Random.Range(minSpinTime, maxSpinTime);
            while (timer < spinTime)
            {
                currentRotation += rotationSpeed * Time.deltaTime;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] != null)
                    {
                        lines[i].transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
                }
                }
                timer += Time.deltaTime;
                yield return null;
            }
            SpawnFire(lines, fireSpawnCount);
            yield return new WaitForSeconds(1f);
        }

        animator.SetBool("isSpinning", false);

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] != null)
                Destroy(lines[i]);
        }

        agent.isStopped = false;
        isAttacking = false;
        yield return new WaitForSeconds(4f);
        canPunch = true;
        
    }

    //eli idea olisi luoda 5 tyhjää gameobjectia viivojen sisälle
    //tasoittaa ne erille toisiistan ja sit spawnata tuli niihin
    //ei hirveän järkevä ratkaisu, jos haluaisi sienon efektin
    private void SpawnFire(GameObject[] lines, int fireCount)
    {
        for (int i = 0; i < lines.Length; i += 2)
        {
            if (lines[i] == null || lines[i + 1] == null) continue;

            Vector3 line1Pos = lines[i].transform.position;
            Vector3 line2Pos = lines[i + 1].transform.position;

            // Midpoint between the two lines
            Vector3 centerPos = (line1Pos + line2Pos) / 2f;
            Vector3 parallelDirection = (line2Pos - line1Pos).normalized;
            Vector3 perpendicularSpread = Vector3.Cross(parallelDirection, Vector3.up).normalized;

            float fireSpacing = 2.0f;
            Quaternion fireRotation = Quaternion.LookRotation(parallelDirection);

            for (int j = 0; j < fireCount; j++)
            {
                float offset = (j + 1) * fireSpacing - (Vector3.Distance(line1Pos, line2Pos) / 2);
                Vector3 fireSpawnPos = centerPos + perpendicularSpread * offset;
                Instantiate(explosion, fireSpawnPos, fireRotation);
            }
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
        agent.enabled = false;
        dead = true;
        agent.isStopped = true;
        isAttacking = true;
        animator.SetBool("Death", true);

        if (dissolveControllers != null)
        {
            foreach (var dissolveController in dissolveControllers)
            {
                Debug.Log("HAHHAHAHAHAaa dissolvea taas!!");
                dissolveController.StartDissolve();
                PlaySound(deathSound);
            }
        }
        else
        {
            Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Destroy(gameObject, deathSound.length);
        }
    }

    public void PlayFootstep()
    {
        if (!sfxSource) return;
        AudioClip stepClip = (step == 0) ? stepSound1 : stepSound2;
        step = 1 - step;

        if (stepClip) sfxSource.PlayOneShot(stepClip);
    }

    private void PlaySound(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}