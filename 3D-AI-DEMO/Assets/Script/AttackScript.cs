using UnityEngine;
using System;
using System.Collections;

public class AttackScript : MonoBehaviour
{
    public StarterAssets.StarterAssetsInputs input;
    public GameObject slash;
    public Transform attackPoint;
    public float attackCooldown = 1f;
    private bool attacking = false;
    private Animator animator;
    private bool _hasAnimator;
    private int _animIDisAttacking;

    void Update()
    {
        _hasAnimator = TryGetComponent(out animator);
        _animIDisAttacking = Animator.StringToHash("isAttacking");
        
        if (input.attack && !attacking)
        {
            StartAttack();
            input.attack = false;
        }
    }

    void StartAttack()
    //IEnumerator StartAttack()
    {
        attacking = true;
        if (_hasAnimator)
        {
            animator.SetBool(_animIDisAttacking, attacking);
        }
        
        Vector3 effectPosition = attackPoint.position;
        Quaternion effectRotation = transform.rotation;
        //yield return new WaitForSeconds(1f);
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
        animator.SetBool(_animIDisAttacking, attacking);
    }
}
