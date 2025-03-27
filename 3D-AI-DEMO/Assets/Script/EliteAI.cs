using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EliteAI : MonoBehaviour
{
    public Transform player;
    private NavMeshAgent agent;
    private Animator animator;

    public float runDistance = 10f;  // Distance at which the AI will start running
    public float walkDistance = 5f;  // Distance at which the AI will start walking

    private float runSpeed = 6f;  // Running speed
    private float walkSpeed = 2f; // Walking speed

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Always move towards the player
        agent.SetDestination(player.position);

        // Get the distance to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // If the player is farther than the "runDistance", set the speed to runSpeed
        if (distanceToPlayer > runDistance)
        {
            agent.speed = runSpeed;
            animator.SetBool("isRunning", true); // Set isRunning to true
            animator.SetBool("isWalking", false); // Set isWalking to false
        }
        // If the player is within the "runDistance" but farther than the "walkDistance", set to walking
        else if (distanceToPlayer > walkDistance)
        {
            agent.speed = walkSpeed;
            animator.SetBool("isWalking", true); // Set isWalking to true
            animator.SetBool("isRunning", false); // Set isRunning to false
        }
    }
}
