using System.Collections;
using UnityEngine;

public class BossDragon : MonoBehaviour
{
    public GameObject fireball;
    public float health = 3f;
    public Transform player;
    private bool isAttacking = false;
    public bool hasAggro = false;
    public Transform fireballSpawn;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!isAttacking && hasAggro)
        {
            Debug.Log("Starting fireball attack...");
            isAttacking = true;
            StartCoroutine(FireballAttackRoutine());
        }
    }

    IEnumerator FireballAttackRoutine()
    {
        animator.SetBool("Attack", true);
        Debug.Log("Attack animation started");

        yield return new WaitForSeconds(0.5f);

        GameObject fireCopy = Instantiate(fireball, fireballSpawn.position, Quaternion.identity);
        Debug.Log("Fireball instantiated");

        // Make fireball a child temporarily while it charges
        fireCopy.transform.SetParent(transform);

        // Grow it large
        Vector3 initialScale = fireCopy.transform.localScale;
        Vector3 targetScale = new Vector3(6f, 6f, 6f); // Much bigger!
        float growDuration = 1.5f;
        float t = 0f;
        while (t < growDuration)
        {
            t += Time.deltaTime;
            fireCopy.transform.localScale = Vector3.Lerp(initialScale, targetScale, t / growDuration);
            yield return null;
        }
        Debug.Log("Fireball charged to full size");

        // Detach and shoot
        fireCopy.transform.SetParent(null);

        Vector3 direction = (player.position - fireballSpawn.position).normalized;
        float speed = 15f;

        Debug.DrawRay(fireballSpawn.position, direction * 5f, Color.red, 2f);
        Debug.Log("Launching fireball toward: " + player.position);

        // Move the fireball using a coroutine
        yield return StartCoroutine(MoveFireball(fireCopy.transform, direction, speed, 3f));

        // Shrink and destroy
        t = 0f;
        Vector3 shrinkScale = Vector3.zero;
        Vector3 currentScale = fireCopy.transform.localScale;
        float shrinkDuration = 0.5f;
        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            fireCopy.transform.localScale = Vector3.Lerp(currentScale, shrinkScale, t / shrinkDuration);
            yield return null;
        }

        Destroy(fireCopy);
        Debug.Log("Fireball destroyed");

        animator.SetBool("Attack", false);
        isAttacking = false;
        Debug.Log("Attack finished");
    }

    IEnumerator MoveFireball(Transform fireball, Vector3 direction, float speed, float duration)
    {
        float timer = 0f;
        while (timer < duration)
        {
            fireball.Translate(direction * speed * Time.deltaTime, Space.World);
            timer += Time.deltaTime;
            yield return null;
        }
    }


}
