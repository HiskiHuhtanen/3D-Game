using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class BossDragon : MonoBehaviour, IDamageable
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
    public float rotationSpeed = 2f;
    public float heightVariation = 1.5f;
    public float bobbingSpeed = 1f;
    public float bankingAmount = 20f;
    private bool attackSpot = false;
    private DissolveController dissolveController;




    void Start()
    {
        animator = GetComponent<Animator>();
        if (pathPoints.Length == 0) return;
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

        chargingFireball = Instantiate(fireballPrefab, fireballSpawn.position, Quaternion.identity);
        chargingFireball.transform.SetParent(fireballSpawn, worldPositionStays: true); // Attach to bone

        // Disable collider and FireDamage script while charging
        Collider fireballCollider = chargingFireball.GetComponent<Collider>();
        if (fireballCollider != null)
            fireballCollider.enabled = false;

        FireDamage fireDamage = chargingFireball.GetComponent<FireDamage>();
        if (fireDamage != null)
            fireDamage.enabled = false;

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

    }

    // Called from animation event to launch the fireball toward the player
    public void LaunchFireball()
{
    if (chargingFireball == null) return;

    chargingFireball.transform.SetParent(null); // Detach from dragon

    // Enable collider and FireDamage script now
    Collider fireballCollider = chargingFireball.GetComponent<Collider>();
    if (fireballCollider != null)
        fireballCollider.enabled = true;

    FireDamage fireDamage = chargingFireball.GetComponent<FireDamage>();
    if (fireDamage != null)
        fireDamage.enabled = true;

    shootDirection = (player.position - chargingFireball.transform.position).normalized;
    chargingFireball.AddComponent<FireballMover>().Initialize(shootDirection, 20f);

    chargingFireball = null; // Clear reference
    animator.SetBool("Attack", false); // Stop the attack animation immediately

    // Start the coroutine to wait 5 seconds before allowing the dragon to move again
    StartCoroutine(WaitAfterAttack());

    // Do not reset attack state immediately
    // isAttacking = false;
    // attackSpot = false;

    // currentPathIndex = (currentPathIndex + 1) % pathPoints.Length; // You can leave this as is or change it based on behavior
}

private IEnumerator WaitAfterAttack()
{
    // Wait for 5 seconds
    yield return new WaitForSeconds(7f);

    // After 5 seconds, reset attack state and continue moving the dragon
    isAttacking = false;
    attackSpot = false;

    // Move to the next path if needed (uncomment if necessary)
    currentPathIndex = (currentPathIndex + 1) % pathPoints.Length;

    // You can also include any other behavior after the delay, e.g., make the dragon fly to the next point
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


    private void OnCollisionEnter(Collision collision)
    {
        // Check if the dragon's collider is hit by a fireball
        if (collision.collider.CompareTag("FIREBALL"))
        {
            FireballMover fireball = collision.collider.GetComponent<FireballMover>();
            if (fireball != null)
            {
                Debug.Log("selvisimme taking damagen sisälle");
                TakeDamage(1f); // Assume fireball has a 'damage' property
                Destroy(collision.gameObject); // Destroy the fireball after it hits
            }
        }
    }


    public void TakeDamage(float amount)
    {
        health -= health;
        if (health == 0)
        {
            Die();
        }
    }


    public void Die()
    {
        hasAggro = false;
        if (dissolveController != null)
        {
            dissolveController.StartDissolve();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
