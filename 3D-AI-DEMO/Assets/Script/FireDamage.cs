using System.Collections;
using UnityEngine;

public class FireDamage : MonoBehaviour
{
    public float damage = 1f;
    public float damageCooldown = 1f;
    private bool canDamage = true;

    private void OnTriggerEnter(Collider other)
    {
        if (canDamage && other.CompareTag("PLAYER"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                StartCoroutine(DamageCooldown());
            }
        }
    }

    private IEnumerator DamageCooldown()
    {
        canDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canDamage = true;
    }
}
