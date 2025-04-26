using TMPro;
using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public float maxHealth = 5f;
    public float health = 5f;
    public TextMeshProUGUI healthText;
    private DissolveController dissolveController;

    public Transform[] respawnPoints; // Set respawn points in inspector

    private StarterAssets.ThirdPersonController controller;
    private CharacterController characterController;
    private SkinnedMeshRenderer[] renderers;
    private Material[][] originalMaterials; // Store original materials

    void Start()
    {
        controller = GetComponent<StarterAssets.ThirdPersonController>();
        characterController = GetComponent<CharacterController>();
        dissolveController = GetComponent<DissolveController>();
        renderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        // Save original materials
        originalMaterials = new Material[renderers.Length][];
        for (int i = 0; i < renderers.Length; i++)
        {
            originalMaterials[i] = renderers[i].materials;
        }

        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        UpdateHealthUI();
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        controller.enabled = false;
        characterController.enabled = false;

        if (dissolveController != null)
        {
            dissolveController.StartDissolve();
        }

        StartCoroutine(RespawnAfterDissolve());
    }

    private IEnumerator RespawnAfterDissolve()
    {
        yield return new WaitForSeconds(dissolveController.delayBeforeDissolve + (2f / dissolveController.dissolveSpeed)); 
        // Adjust based on dissolve speed/time
        yield return new WaitForSeconds(dissolveController.delayBeforeDelete);

        Respawn();
    }

    void Respawn()
    {
        Transform respawnPoint = GetNearestRespawnPoint();
        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }

        // Restore original materials
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && originalMaterials[i] != null)
            {
                renderers[i].materials = originalMaterials[i];
            }
        }
        health = maxHealth;
        UpdateHealthUI();
        controller.enabled = true;
        characterController.enabled = true;
    }

    Transform GetNearestRespawnPoint()
    {
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (Transform point in respawnPoints)
        {
            float dist = Vector3.Distance(transform.position, point.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearest = point;
            }
        }

        return nearest;
    }

    void UpdateHealthUI()
    {
        healthText.text = "Health: " + health.ToString();
    }
}
