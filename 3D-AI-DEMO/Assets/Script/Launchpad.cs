using UnityEngine;

public class Launchpad : MonoBehaviour
{
    public Transform targetPoint;
    public float arcHeight = 5f;
    public float launchDelay = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PLAYER"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                StartCoroutine(LaunchAfterDelay(rb));
            }
        }
    }

    private System.Collections.IEnumerator LaunchAfterDelay(Rigidbody playerRb)
    {
        yield return new WaitForSeconds(launchDelay);
        Vector3 velocity = CalculateLaunchVelocity(transform.position, targetPoint.position, arcHeight);
        playerRb.linearVelocity = velocity;
    }

    //gpt matikkaa taas
    private Vector3 CalculateLaunchVelocity(Vector3 startPoint, Vector3 endPoint, float arcHeight)
    {
        
        Vector3 displacement = endPoint - startPoint;
        Vector3 displacementXZ = new Vector3(displacement.x, 0, displacement.z);

        float gravity = Mathf.Abs(Physics.gravity.y);
    
        float heightDifference = endPoint.y - startPoint.y;
        float clampedArcHeight = Mathf.Max(arcHeight, heightDifference + 0.1f); // ensure it's high enough
        Debug.Log($"Start: {startPoint}, Target: {endPoint}, HeightDiff: {heightDifference}");

        float timeUp = Mathf.Sqrt(2 * clampedArcHeight / gravity);
        float timeDown = Mathf.Sqrt(2 * (clampedArcHeight - heightDifference) / gravity);
        float totalTime = timeUp + timeDown;

        if (float.IsNaN(totalTime) || totalTime <= 0f)
        {
            Debug.LogWarning("Invalid launch calculation: arc or target too low?");
            return Vector3.zero;
        }

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(2 * gravity * clampedArcHeight);
        Vector3 velocityXZ = displacementXZ / totalTime;

        return velocityXZ + velocityY;
    }
}
