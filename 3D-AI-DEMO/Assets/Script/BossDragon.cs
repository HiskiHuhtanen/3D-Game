using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class BossDragon : MonoBehaviour
{
    public GameObject fireballPrefab;
    public float health = 3f;
    public Transform player;
    public bool hasAggro = false;
    public Transform fireballSpawn;
    private GameObject chargingFireball;
    private Animator animator;
    private bool isAttacking = false;
    private Vector3 shootDirection;
    public Transform[] pathPoints;
    private int currentPathIndex = 0;
    public float speed = 10f;
    private Vector3 velocity = Vector3.zero;
    private float originalY;
    public float rotationSpeed = 2f;
    public float heightVariation = 1.5f;
    public float bobbingSpeed = 1f;
    public float bankingAmount = 20f;
    private bool attackSpot = false;




    void Start()
    {
        animator = GetComponent<Animator>();
        if (pathPoints.Length == 0) return;
        originalY = transform.position.y;
    }

    void Update()
{
    if (!isAttacking && hasAggro && !attackSpot)
    {
        MoveSmoothlyAlongPath();
    }

    if (attackSpot)
    {
        attackSpot = false;
        Debug.Log("Starting fireball attack...");
        isAttacking = true;
        animator.SetBool("Attack", true);
    }

    if (isAttacking)
    {
        LookAtPlayer();
    }
}


    // Called from animation event to create and grow the fireball
    public void SpawnAndGrowFireball()
    {
        if (chargingFireball != null) return; // Already charging

        Debug.Log("Spawning and growing fireball...");

        chargingFireball = Instantiate(fireballPrefab, fireballSpawn.position, Quaternion.identity);
        chargingFireball.transform.SetParent(fireballSpawn, worldPositionStays: true); // Attach to bone

        StartCoroutine(GrowFireball(chargingFireball.transform));
    }

    IEnumerator GrowFireball(Transform fireball)
    {
        Vector3 initialScale = fireball.localScale;
        Vector3 targetScale = new Vector3(20f, 20f, 20f);
        float growDuration = 1.5f;
        float t = 0f;

        while (t < growDuration)
        {
            t += Time.deltaTime;
            fireball.localScale = Vector3.Lerp(initialScale, targetScale, t / growDuration);
            yield return null;
        }

        Debug.Log("Fireball fully charged.");
    }

    // Called from animation event to launch the fireball toward the player
    public void LaunchFireball()
    {
        if (chargingFireball == null) return;

        Debug.Log("Launching fireball...");

        chargingFireball.transform.SetParent(null); // Detach from dragon

        shootDirection = (player.position - chargingFireball.transform.position).normalized;
        chargingFireball.AddComponent<FireballMover>().Initialize(shootDirection, 20f);

        chargingFireball = null; // Clear reference
        animator.SetBool("Attack", false);
        isAttacking = false;
        attackSpot = false;
        Debug.Log("Attack finished, moving to next path point.");

        // FORCE MOVE TO NEXT PATH POINT
        currentPathIndex = (currentPathIndex + 1) % pathPoints.Length;
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

        if (!attackSpot)
        {
            if (currentPathIndex == 3 || currentPathIndex == 6)
            {
                attackSpot = true;
            }
        }
    }


    private void LookAtPlayer()
{
    Vector3 directionToPlayer = (player.position - transform.position).normalized;
    directionToPlayer.y = 0; // Keep dragon level

    if (directionToPlayer != Vector3.zero)
    {
        Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotationSpeed * 2f * Time.deltaTime); // faster turn when attacking
    }
}



}
