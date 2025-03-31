using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
    public bool canPunch = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    //gpt koodia, oma ei toiminut
    //tiedän miksi, en jaksanut korjata
    //kello on yö.

    //hahaaa! ei enää
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
        else if (distanceToPlayer <= rangedAttackRange) 
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
    Debug.Log("Punch coroutine started");
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
        Debug.Log(rangedAttackCounter);
    }

    //HOX HOX! Gpt auttoi tässä matikassa
    private IEnumerator SpinAttack()
    {
        isAttacking = true;
        agent.isStopped = true;
        Debug.Log("SPIIIIN");

        int pairs = 8;
        float lineDistance = 5f;
        float rotationSpeed = 45f;
        GameObject[] lines = new GameObject[pairs * 2];
        float angleStep = 360f / pairs;
        float currentRotation = 0f;
        int attackRepeats = 3;
        int fireSpawnCount = 5;
        float minSpinTime = 1.5f;
        float maxSpinTime = 3f;

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
        
    }

    //TÄMÄ EI TOIMI
    //KYLLÄ POHJA OLI VAA TEKOÄLYLTÄ, PARANNAN KUN JAKSAN
    //eli idea olisi luoda 5 tyhjää gameobjectia viivojen sisälle
    //tasoittaa ne eriin toisiistan ja sit spawnata tuli niihin
    //ei hirveän järkevä ratkaisu, jos haluaisi sienon efektin
    private void SpawnFire(GameObject[] lines, int fireCount)
    {
        Debug.Log("TULTAAAAAAAAAA!!!!!!");
        List<Vector3> firePositions = new List<Vector3>();

        for (int i = 0; i < lines.Length; i += 2) // Each pair of lines
        {
            if (lines[i] == null || lines[i + 1] == null) continue;

            Vector3 start = lines[i].transform.position;
            Vector3 end = lines[i + 1].transform.position;

            // Find the direction vector perpendicular to the lines
            Vector3 midPoint = (start + end) / 2f; // Center between the two lines
            Vector3 perpendicular = (end - start).normalized; // Direction between the lines
            Vector3 spreadDirection = Vector3.Cross(perpendicular, Vector3.up); // Perpendicular to it

            for (int j = 0; j < fireCount; j++)
            {
                float offset = ((j - (fireCount / 2f)) / fireCount) * (Vector3.Distance(start, end)); 
                Vector3 firePos = midPoint + spreadDirection * offset; // Spread along perpendicular axis
                firePositions.Add(firePos);
            }
        }

        // Spawn fire effects at calculated positions
        foreach (Vector3 pos in firePositions)
        {
            Instantiate(explosion, pos, Quaternion.identity);
        }
    }
}