using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FireballMover : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private bool isReflected = false; // NEW
    private Transform dragonTarget; // NEW
    public BossDragon BossDragon;


    public void Initialize(Vector3 dir, float spd)
    {
        direction = dir;
        speed = spd;
        Destroy(gameObject, 7f); // Destroy after 5 seconds if missed
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    public void SetBossDragon(BossDragon dragon)
    {
        BossDragon = dragon;
    }

    public void Reflect(Vector3 newDirection, Transform dragon)
    {
        Debug.Log("miumau");
        direction = newDirection.normalized;
        isReflected = true;
        dragonTarget = dragon;

        transform.forward = direction; // Rotate visuals toward new direction

        // Now instead of instantly applying damage, start coroutine:
        StartCoroutine(DelayedDamage());
    }

    IEnumerator DelayedDamage()
    {
        yield return new WaitForSeconds(2f); // <-- wait 2 seconds (or any value you want)

        if (BossDragon != null)
        {
            BossDragon.TakeDamage(1f);
            Debug.Log("Fireball delayed hit! Dragon taking damage.");

            if (BossDragon.health <= 0)
            {
                Debug.Log("Dragon dying");
                BossDragon.Die();
            }
        }
    }


}
