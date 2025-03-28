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
    public bool canPunch = true;

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

        if (distanceToPlayer <= attackRange && canPunch) 
        {
            StartCoroutine(Punch());
        }
        else if (distanceToPlayer <= rangedAttackRange) 
        {
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
    }
}
