using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


//sisältää PALJON turhia rivejä, oletan, en ole tarkistanut
//muutenkin nykyinen toiminta ja rakenne on hieman outo.

public class EliteAI : MonoBehaviour
{
    public Transform player;
    public GameObject fire;
    public GameObject warningLine;
    public GameObject explosion;
    private int rangeCooldown = 0;
    private int rangedAttackCounter = 0;
    private NavMeshAgent agent;
    private Animator animator;

    public float runDistance = 10f;
    public float walkDistance = 5f;

    private float runSpeed = 6f;
    private float walkSpeed = 2f;
    public float attackRange = 4f;   
    public float rangedAttackRange = 20f; 
    private bool isAttacking = false;
    public bool canPunch = true; //muuta nimi jossain vaiheessa globaaliksi attackCooldowniksi

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    //ei toimi, tiedän miksi, en jaksanut korjata
    //kello on yö.

    //hahaaa! ei enää!
    void Update()
    {
        if (isAttacking) return; 

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
    isAttacking = true;
    canPunch = false;

    animator.SetBool("isDashing", true);

    Vector3 firstDashDirection = (transform.forward + transform.right).normalized;
    Vector3 firstDashPosition = transform.position + firstDashDirection * 6f; 
    transform.position = firstDashPosition;
    yield return new WaitForSeconds(0.5f);

    Vector3 directionToPlayer = (player.transform.position - firstDashPosition).normalized;
    Vector3 secondDashPosition = player.transform.position + directionToPlayer * -2f;
    transform.position = secondDashPosition;
    //yield return new WaitForSeconds(0.5f);

    animator.SetBool("isDashing", false);
    animator.SetBool("isPunching", true);
    agent.isStopped = true; 
    yield return new WaitForSeconds(2f);

    animator.SetBool("isPunching", false);

    agent.isStopped = false;
    isAttacking = false;
    //miksi näin???? eikä vaa rangeCooldown -= 1
    rangeCooldown = Mathf.Max(rangeCooldown - 1, 0);
    yield return new WaitForSeconds(4f);
    canPunch = true;
    }

    private IEnumerator FireAttack()
    {
        canPunch = false;
        isAttacking = true;
        agent.isStopped = true;
        animator.SetBool("isRanged", true);
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
        canPunch = false;
        isAttacking = true;
        agent.isStopped = true;
        Debug.Log("SPIIIIN");

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

    //TÄMÄ EI TOIMI
    //KYLLÄ POHJA OLI VAA TEKOÄLYLTÄ, PARANNAN KUN JAKSAN
    //eli idea olisi luoda 5 tyhjää gameobjectia viivojen sisälle
    //tasoittaa ne eriin toisiistan ja sit spawnata tuli niihin
    //ei hirveän järkevä ratkaisu, jos haluaisi sienon efektin
    private void SpawnFire(GameObject[] lines, int fireCount)
    {
        for (int i = 0; i < lines.Length; i += 2) // Each pair of parallel lines
        {
            if (lines[i] == null || lines[i + 1] == null) continue;

            Vector3 line1Pos = lines[i].transform.position;
            Vector3 line2Pos = lines[i + 1].transform.position;

            // Midpoint between the two lines
            Vector3 centerPos = (line1Pos + line2Pos) / 2f;

            // Direction of fire spread (same as FireAttack)
            Vector3 forwardDirection = transform.forward; 

            Vector3 parallelDirection = (line2Pos - line1Pos).normalized;
            Vector3 perpendicularSpread = Vector3.Cross(parallelDirection, Vector3.up).normalized;

            float fireSpacing = 2.0f; // Adjust to change fire spread
            //Quaternion fireRotation = Quaternion.LookRotation(forwardDirection) * Quaternion.Euler(0, 90, 0);
            Quaternion fireRotation = Quaternion.LookRotation(parallelDirection);

            for (int j = 0; j < fireCount; j++)
            {
                float offset = (j + 1) * fireSpacing - (Vector3.Distance(line1Pos, line2Pos) / 2);
                Vector3 fireSpawnPos = centerPos + perpendicularSpread * offset;
                Instantiate(explosion, fireSpawnPos, fireRotation);
            }
        }
    }
}