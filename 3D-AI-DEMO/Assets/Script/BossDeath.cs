using UnityEngine;

public class BossDeath : MonoBehaviour
{
    public Material skyMaterial;
    public Color startColor = new Color(0xdd / 255f, 0x00 / 255f, 0x20 / 255f);
    public Color endColor = new Color(0xa0 / 255f, 0x51 / 255f, 0xcf / 255f);
    public float transitionDuration = 5f;

    private float timer = 0f;
    private bool isTransitioning = false;

    void Start()
    {
        if (skyMaterial != null)
        {
            skyMaterial.SetColor("_HorizonLineColor", startColor);
        }
    }

    void Update()
    {
        if (isTransitioning && skyMaterial != null)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / transitionDuration);
            Color currentColor = Color.Lerp(startColor, endColor, t);
            skyMaterial.SetColor("_HorizonLineColor", currentColor);

            if (t >= 1f)
                isTransitioning = false;
        }
    }

    // Call this method to trigger the transition
    public void TriggerBossDeath()
    {
        timer = 0f;
        isTransitioning = true;
    }
}
