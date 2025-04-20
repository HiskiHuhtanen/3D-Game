using UnityEngine;
using UnityEngine.AI;

public class ChainLimiter : MonoBehaviour
{
    public Transform chainEnd; // The final chain link's Transform
    public float maxChainLength = 5f; // Total max length your chain allows

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        float distanceToChain = Vector3.Distance(transform.position, chainEnd.position);

        if (distanceToChain > maxChainLength)
        {
            agent.isStopped = true;
        }
        else
        {
            agent.isStopped = false;
        }
    }
}
