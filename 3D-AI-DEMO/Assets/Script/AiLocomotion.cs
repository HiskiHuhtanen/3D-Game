using UnityEngine;
using UnityEngine.AI;

public class AiLocomotion : MonoBehaviour
{

    public Transform player;
    NavMeshAgent agent;
    Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        agent.destination = player.position;
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}
