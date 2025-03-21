using UnityEngine;

public class ProjectileHit : MonoBehaviour
{
    public float damage = 1f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PLAYER"))
        {
            Player player = other.GetComponent<Player>();
            if (player != null)
            {
                player.TakeDamage(damage);
                Debug.Log("Player hit by projectile!");
            }

            Destroy(gameObject);
        }
    }
}
