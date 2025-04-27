using UnityEngine;
using System.Collections;

public class AttackScript : MonoBehaviour
{
    public StarterAssets.StarterAssetsInputs input;
    public GameObject slash;
    public Transform attackPoint;
<<<<<<< HEAD
    public float attackCooldown = 0f;
=======
    public Transform dragonSpot;
    public float attackCooldown = 0.1f;
>>>>>>> b7297a032c9fd61a7ee86b30151ccb4112886809
    private bool attacking = false;
    private Animator animator;
    private bool _hasAnimator;
    private int _animIDisAttacking;

    void Start()
    {
        _hasAnimator = TryGetComponent(out animator);
        _animIDisAttacking = Animator.StringToHash("isAttacking");
    }

    void Update()
    { 
        if (input.attack && !attacking)
        {
            StartCoroutine(StartAttack());
            input.attack = false;
        }
    }

    IEnumerator StartAttack()
    {
        attacking = true;
        if (_hasAnimator)
        {
            animator.SetBool(_animIDisAttacking, attacking);
        }

        Vector3 effectPosition = attackPoint.position;
        Quaternion effectRotation = transform.rotation;
<<<<<<< HEAD
        yield return new WaitForSeconds(0.4f);
=======

        yield return new WaitForSeconds(0.3f); // wait before slash effect appears

>>>>>>> b7297a032c9fd61a7ee86b30151ccb4112886809
        GameObject effect = Instantiate(slash, effectPosition, effectRotation);
        Destroy(effect, 1.0f);

        Collider[] hitObjects = Physics.OverlapSphere(attackPoint.position, 2f);
        foreach (Collider hit in hitObjects)
        {
            if (hit.CompareTag("Enemy"))
            {
                if (hit.TryGetComponent<IDamageable>(out var target))
                {
                    Debug.Log("Enemy took damage!");
                    target.TakeDamage(1f);
                }
            }
            else if (hit.CompareTag("FIREBALL")) // ✨ Reflect fireballs
            {
                if (hit.TryGetComponent<FireballMover>(out var fireball))
                {
                    Debug.Log("Fireball reflected!");
                    Vector3 reflectDirection = (fireball.transform.position - transform.position).normalized;
                    Transform dragon = FindObjectOfType<BossDragon>().transform;
                    fireball.Reflect(reflectDirection, dragon);
                }
            }
        }

        Invoke(nameof(ResetAttack), attackCooldown);
    }

    void ResetAttack()
    {
        attacking = false;
        if (_hasAnimator)
        {
            animator.SetBool(_animIDisAttacking, attacking);
        }
    }
}
