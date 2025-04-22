using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image _healthbarSprite;
    private float _reduceSpeed = 2;
    private float _target = 1;
    private Camera _cam;

    void Start()
    {
        _cam = Camera.main;
    }

    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position);
        _healthbarSprite.fillAmount = Mathf.MoveTowards(_healthbarSprite.fillAmount, _target, _reduceSpeed * Time.deltaTime);
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        _target = Mathf.Clamp01(currentHealth / maxHealth);
    }
}
