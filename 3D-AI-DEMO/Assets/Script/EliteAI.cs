using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


//sisältää PALJON turhia rivejä, oletan, en ole tarkistanut
//muutenkin nykyinen toiminta ja rakenne on hieman outo.

public class EliteAI : MonoBehaviour, IDamageable
{
    public Transform player;
    public GameObject fire;
    public GameObject warningLine;
    public GameObject explosion;
    public AudioClip deathSound;
    public float health = 1f;
    private int rangeCooldown = 0;
    private int rangedAttackCounter = 0;
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;

    public float runDistance = 10f;
    public float walkDistance = 5f;

    private float runSpeed = 6f;
    private float walkSpeed = 2f;
    public float attackRange = 4f;   
    public float rangedAttackRange = 20f; 
    private bool isAttacking = false;
    public bool canPunch = true; //muuta nimi jossain vaiheessa globaaliksi attackCooldowniksi
    private float dissolveValue = -1f;
    private float dissolveSpeed = 1f;
    private Material _instanceMaterial;
    private bool isDissolving = false;
    public Material dissolveMat;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (dissolveMat != null)
        {
            SkinnedMeshRenderer meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (meshRenderer != null)
            {
                Material originalMat = meshRenderer.material;
                _instanceMaterial = new Material(dissolveMat);

                // Copy textures and color from original material
                if (originalMat.HasProperty("_BaseMap"))
                    _instanceMaterial.SetTexture("_BaseMap", originalMat.GetTexture("_BaseMap"));
                if (originalMat.HasProperty("_NormalMap"))
                    _instanceMaterial.SetTexture("_NormalMap", originalMat.GetTexture("_NormalMap"));
                if (originalMat.HasProperty("_EmissionMap"))
                    _instanceMaterial.SetTexture("_EmissionMap", originalMat.GetTexture("_EmissionMap"));
                if (originalMat.HasProperty("_Color"))
                    _instanceMaterial.SetColor("_Color", originalMat.GetColor("_Color"));
                if (originalMat.IsKeywordEnabled("_EMISSION"))
                    _instanceMaterial.EnableKeyword("_EMISSION");

                _instanceMaterial.SetFloat("_DissolveAmount", dissolveValue);

                meshRenderer.material = _instanceMaterial;
            }
            else
            {
                Debug.LogWarning("EII TOIMI! (SkinnedMeshRenderer not found)");
            }
        }
    }

    //ei toimi, tiedän miksi, en jaksanut korjata
    //kello on yö.

    //hahaaa! ei enää!
    void Update()
    {
        if (isDissolving)
        {
            agent.isStopped = true;
            dissolveValue += Time.deltaTime * dissolveSpeed;
            _instanceMaterial.SetFloat("_DissolveAmount", dissolveValue);
        }
        if (isAttacking) return; 

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        animator.SetBool("isRunning", true);

        agent.SetDestination(player.position);

        if (distanceToPlayer <= attackRange && canPunch) 
        {
            StartCoroutine(Punch());
        }
        else if (distanceToPlayer <= rangedAttackRange && canPunch) 
        {
            if (distanceToPlayer <= rangedAttackRange && rangeCooldown == 0) 
            {
                if (rangedAttackCounter < 1)
                {
                    StartCoroutine(FireAttack());
                }
                else
                {
                    StartCoroutine(SpinAttack());
                    rangedAttackCounter = 0;
                }
            }
        }
    }

    private IEnumerator Punch()
    {
    isAttacking = true;
    canPunch = false;

    animator.SetBool("isDashing", true);

    Vector3 firstDashDirection = (transform.forward + transform.right).normalized;
    Vector3 firstDashPosition = transform.position + firstDashDirection * 6f; 
    transform.position = firstDashPosition;
    yield return new WaitForSeconds(0.5f);

    Vector3 directionToPlayer = (player.transform.position - firstDashPosition).normalized;
    Vector3 secondDashPosition = player.transform.position + directionToPlayer * -2f;
    transform.position = secondDashPosition;
    //yield return new WaitForSeconds(0.5f);

    animator.SetBool("isDashing", false);
    animator.SetBool("isPunching", true);
    agent.isStopped = true; 
    yield return new WaitForSeconds(2f);

    animator.SetBool("isPunching", false);

    agent.isStopped = false;
    isAttacking = false;
    //miksi näin???? eikä vaa rangeCooldown -= 1
    rangeCooldown = Mathf.Max(rangeCooldown - 1, 0);
    yield return new WaitForSeconds(4f);
    canPunch = true;
    }

    private IEnumerator FireAttack()
    {
        canPunch = false;
        isAttacking = true;
        agent.isStopped = true;
        animator.SetBool("isRanged", true);
        int fireCount = 10;
        float fireDistance = 1.5f;
        rangeCooldown = rangeCooldown + 2;
        
        for (int i = 0; i < fireCount; i++)
        {
            Vector3 fireSpawnPos = transform.position + transform.forward * (i * fireDistance + 1.5f);
            GameObject fireCopy = Instantiate(fire, fireSpawnPos, Quaternion.identity);
            Destroy(fireCopy, 5);
            yield return new WaitForSeconds(0.2f);  
        }
        animator.SetBool("isRanged", false);
        yield return new WaitForSeconds(1f);
        agent.isStopped = false;
        isAttacking = false;
        rangedAttackCounter += 1;
        yield return new WaitForSeconds(3f);
        canPunch = true;

    }

    //HOX HOX! Gpt auttoi tässä matikassa
    private IEnumerator SpinAttack()
    {
        canPunch = false;
        isAttacking = true;
        agent.isStopped = true;
        Debug.Log("SPIIIIN");

        animator.SetTrigger("SpinStart");
        yield return new WaitForSeconds(0.8f);

        animator.SetBool("isSpinning" , true);

        //näitä voisi ehkä siirtää publiciksi
        int pairs = 8;
        float lineDistance = 5f;
        float rotationSpeed = 45f;
        GameObject[] lines = new GameObject[pairs * 2];
        float angleStep = 360f / pairs;
        float currentRotation = 0f;
        int attackRepeats = 3;
        int fireSpawnCount = 10;
        float minSpinTime = 1.5f;
        float maxSpinTime = 5f;

        for (int i = 0; i < pairs; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
            Vector3 offset = Vector3.Cross(dir, Vector3.up) * 1f;
            Vector3 pos1 = transform.position + dir * lineDistance + offset;
            Vector3 pos2 = transform.position + dir * lineDistance - offset;

            // Rotate 90 degrees around the Y-axis so the lines stand upright
            lines[i * 2] = Instantiate(warningLine, pos1, Quaternion.LookRotation(dir) * Quaternion.Euler(90, 0, 0));
            lines[i * 2 + 1] = Instantiate(warningLine, pos2, Quaternion.LookRotation(dir) * Quaternion.Euler(90, 0, 0));

        }
        for (int repeat = 0; repeat < attackRepeats; repeat++)
        {
            float timer = 0f;
            float spinTime = UnityEngine.Random.Range(minSpinTime, maxSpinTime);
            while (timer < spinTime)
            {
                currentRotation += rotationSpeed * Time.deltaTime;
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i] != null)
                    {
                        lines[i].transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
                }
                }
                timer += Time.deltaTime;
                yield return null;
            }
            SpawnFire(lines, fireSpawnCount);
            yield return new WaitForSeconds(1f);
        }

        animator.SetBool("isSpinning", false);

        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i] != null)
                Destroy(lines[i]);
        }

        agent.isStopped = false;
        isAttacking = false;
        yield return new WaitForSeconds(4f);
        canPunch = true;
        
    }

    //eli idea olisi luoda 5 tyhjää gameobjectia viivojen sisälle
    //tasoittaa ne erille toisiistan ja sit spawnata tuli niihin
    //ei hirveän järkevä ratkaisu, jos haluaisi sienon efektin
    private void SpawnFire(GameObject[] lines, int fireCount)
    {
        for (int i = 0; i < lines.Length; i += 2)
        {
            if (lines[i] == null || lines[i + 1] == null) continue;

            Vector3 line1Pos = lines[i].transform.position;
            Vector3 line2Pos = lines[i + 1].transform.position;

            // Midpoint between the two lines
            Vector3 centerPos = (line1Pos + line2Pos) / 2f;
            Vector3 parallelDirection = (line2Pos - line1Pos).normalized;
            Vector3 perpendicularSpread = Vector3.Cross(parallelDirection, Vector3.up).normalized;

            float fireSpacing = 2.0f;
            Quaternion fireRotation = Quaternion.LookRotation(parallelDirection);

            for (int j = 0; j < fireCount; j++)
            {
                float offset = (j + 1) * fireSpacing - (Vector3.Distance(line1Pos, line2Pos) / 2);
                Vector3 fireSpawnPos = centerPos + perpendicularSpread * offset;
                Instantiate(explosion, fireSpawnPos, fireRotation);
            }
        }
    }
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        agent.isStopped = true;
        isAttacking = true;
        animator.SetBool("Death", true);
        PlayDeathSound();

        if (_instanceMaterial != null)
        {
            isDissolving = true;
            SkinnedMeshRenderer meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.materials = new Material[] {_instanceMaterial};
            }
            StartCoroutine(Dissolve(5f , 5f));
        }
        else
        {
            Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            Destroy(gameObject, deathSound.length);
        }
    }

    private void PlayDeathSound()
    {
        if (audioSource && deathSound)
        {
            audioSource.PlayOneShot(deathSound);
        }
    }

    private IEnumerator Dissolve(float delayBeforeDissolve, float dissolveDuration)
    {
        yield return new WaitForSeconds(delayBeforeDissolve);

        float elapsed = 0f;
        isDissolving = true;

        SkinnedMeshRenderer meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogWarning("No SkinnedMeshRenderer found");
            yield break;
        }

        Material[] materials = meshRenderer.materials;
        while (elapsed < dissolveDuration)
        {
            float dissolveAmount = Mathf.Lerp(0f, 1f, elapsed / dissolveDuration);
            foreach (var mat in materials)
            {
                if (mat.HasProperty("_DissolveAmount"))
                    mat.SetFloat("_DissolveAmount", dissolveAmount);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        foreach (var mat in materials)
        {
            if (mat.HasProperty("_DissolveAmount"))
                mat.SetFloat("_DissolveAmount", 1f);
        }
        Destroy(gameObject);
    }

}