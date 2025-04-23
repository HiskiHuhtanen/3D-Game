using System.Collections;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    public Transform[] pathPoints;
    private int currentPathIndex = 0;
    public float speed = 10f;
    public float rotationSpeed = 2f;
    public float heightVariation = 1.5f;
    public float bobbingSpeed = 1f;
    public float bankingAmount = 20f;

    private Vector3 velocity = Vector3.zero;
    private float originalY;

    void Start()
    {
        if (pathPoints.Length == 0) return;
        originalY = transform.position.y;
    }

    void Update()
    {
        MoveSmoothlyAlongPath();
    }

    void MoveSmoothlyAlongPath()
    {
        if (pathPoints.Length == 0) return;

        Transform target = pathPoints[currentPathIndex];
        Vector3 targetPosition = target.position;

        // Add vertical bobbing to simulate soaring
        float bobbingOffset = Mathf.Sin(Time.time * bobbingSpeed) * heightVariation;
        targetPosition.y += bobbingOffset;

        // Smooth position movement
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 0.3f, speed);

        // Smooth rotation to look at the direction of motion
        Vector3 direction = (target.position - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Add banking effect when turning
            float banking = Mathf.Clamp(Vector3.SignedAngle(transform.forward, direction, Vector3.up), -1f, 1f) * bankingAmount;
            targetRotation *= Quaternion.Euler(0, 0, -banking);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Move to next path point when close enough
        if (Vector3.Distance(transform.position, target.position) < 2f)
        {
            currentPathIndex = (currentPathIndex + 1) % pathPoints.Length;
        }
    }
}
