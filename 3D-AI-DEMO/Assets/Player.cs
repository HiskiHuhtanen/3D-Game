using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float health = 5f;
    public float currentHealth;
    public TextMeshProUGUI healthText;

    void Start()
    {
        UpdateHealthUI();
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        UpdateHealthUI();
        if (health <= 0) {
            Die();
        }
    }
    void Die()
    {
        Debug.Log("Player has died!");
        
        GetComponent<StarterAssets.ThirdPersonController>().enabled = false;
        GetComponent<CharacterController>().enabled = false;

        Destroy(gameObject, 2f);
    }

    void UpdateHealthUI()
    {
        healthText.text = "Health: " + health.ToString();
    }

}
