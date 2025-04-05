using UnityEngine;

public class AttackScript : MonoBehaviour
{
    public StarterAssets.StarterAssetsInputs input;
    public GameObject slash;
    public Transform attackPoint;
    public float attackCooldown = 1f;
    private bool attacking = false;

    void Update()
    {
        if (input.attack && !attacking)
        {
            StartAttack();
            input.attack = false;
        }
    }

    void StartAttack()
    {
        attacking = true;
        Vector3 effectPosition = attackPoint.position;
        Quaternion effectRotation = transform.rotation;
        GameObject effect = Instantiate(slash, effectPosition, effectRotation);
        Destroy(effect, 1.0f);

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, 1f);
        foreach (Collider enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                if (enemy.TryGetComponent<IDamageable>(out var target))
                {
                    Debug.Log("Enemy took damage!");
                    target.TakeDamage(1f);
                }
            }
        }

        Invoke(nameof(ResetAttack), attackCooldown);
    }

    void ResetAttack()
    {
        attacking = false;
    }
}
