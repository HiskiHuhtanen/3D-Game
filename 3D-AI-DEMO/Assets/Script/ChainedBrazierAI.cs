using UnityEngine;
using UnityEngine.AI;

public class ChainedBrazierAI : BrazierAI
{
    public Transform chainStart;
    public float maxChainLength = 5f;
    public float stuckTimeBeforeRedirect = 1.5f;
    public Rigidbody finalChainLink;

    private float stuckTimer = 0f;
    private bool chainBroken = false;

    protected override void Update()
    {
        float distanceToStart = Vector3.Distance(transform.position, chainStart.position);

        // Break free if aggro triggers and we haven't already
        if (!chainBroken && hasAggro)
        {
            BreakChain();
        }

        // While chain is still connected, apply constraint
        if (!chainBroken && distanceToStart > maxChainLength)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer >= stuckTimeBeforeRedirect)
            {
                Vector3 fallback = RandomNavSphere(transform.position, wanderRadius, -1);
                agent.SetDestination(fallback);
                stuckTimer = 0f;
            }

            return; // Don't continue AI logic if stuck at edge
        }

        stuckTimer = 0f;

        base.Update();
    }

    private void BreakChain()
    {
        if (finalChainLink != null)
        {
            var joint = finalChainLink.GetComponent<Joint>();
            if (joint != null)
            {
                Destroy(joint); // Let the chain fall
            }
        }

        chainBroken = true;
    }
}
