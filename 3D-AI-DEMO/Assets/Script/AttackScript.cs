using UnityEngine;

public class AttackScript : MonoBehaviour
{
    public StarterAssets.StarterAssetsInputs input;
    public GameObject slash;
    public Transform attackPoint;
    public float attackCooldown = 1f;
    private bool attacking = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
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
            Debug.Log("Thing took damage!");
    
            if (enemy.TryGetComponent<MeleeTrigger>(out var meleeEnemy))
            {
            meleeEnemy.TakeDamage(1f);
            }
            else if (enemy.TryGetComponent<AiRangedEnemy>(out var rangedEnemy))
            {
            rangedEnemy.TakeDamage(1f);
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
