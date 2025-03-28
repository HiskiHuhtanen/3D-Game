using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EliteAI : MonoBehaviour
{
    public Transform player;
    public GameObject fire;
    private int rangeCooldown = 0;
    private NavMeshAgent agent;
    private Animator animator;

    public float runDistance = 10f;
    public float walkDistance = 5f;

    private float runSpeed = 6f;
    private float walkSpeed = 2f;
    public float attackRange = 4f;   
    public float rangedAttackRange = 20f; 
    private bool isAttacking = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    //gpt koodia, oma ei toiminut
    //tiedän miksi, en jaksanut korjata
    //kello on yö.
    void Update()
    {
        if (isAttacking) return; 

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        animator.SetBool("isRunning", true);

        agent.SetDestination(player.position);

        if (distanceToPlayer <= attackRange) 
        {
            StartCoroutine(Punch());
        }
        else if (distanceToPlayer <= rangedAttackRange) 
        {
            // 50% chance to do ranged attack if both attacks are possible
            //EI tämä tee enää mitään järkee!!!, poista....
            if (distanceToPlayer <= rangedAttackRange && rangeCooldown == 0) 
            {
                StartCoroutine(FireAttack());
            }
        }
    }

    private IEnumerator Punch()
    {
    Debug.Log("Punch coroutine started");
    isAttacking = true;

    // First teleport outward (left or right) with a greater distance
    Debug.Log("First teleport outward...");
    animator.SetBool("isDashing", true);
    Vector3 firstDashDirection = Random.Range(0, 2) == 0 ? transform.right : -transform.right;
    Vector3 firstDashPosition = transform.position + firstDashDirection * 6f; // Increase outward teleport distance
    transform.position = firstDashPosition;
    yield return new WaitForSeconds(0.5f);

    // Second teleport inward towards the player with a greater distance
    Debug.Log("Second teleport inward towards player...");
    Vector3 directionToPlayer = (player.transform.position - firstDashPosition).normalized;
    Vector3 secondDashPosition = firstDashPosition + directionToPlayer * 8f; // Increase inward teleport distance
    transform.position = secondDashPosition;
    yield return new WaitForSeconds(0.5f);

    animator.SetBool("isDashing", false);
    animator.SetBool("isPunching", true);

    // Stop agent for attack
    Debug.Log("Stopping agent for attack...");
    agent.isStopped = true; 
    yield return new WaitForSeconds(3f);

    animator.SetBool("isPunching", false);

    // Resume movement
    Debug.Log("Attack complete, resuming movement...");
    agent.isStopped = false;
    isAttacking = false;
    rangeCooldown = Mathf.Max(rangeCooldown - 1, 0);
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
    }
}
