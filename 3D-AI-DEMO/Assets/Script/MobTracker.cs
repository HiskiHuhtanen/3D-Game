using UnityEngine;
using System.Collections.Generic;

public class MobTracker : MonoBehaviour
{
    public List<GameObject> mobsToWatch = new List<GameObject>();
    public ShaderPosition shaderPosition;
    public float targetRadius = 10f;
    public float growSpeed = 1f;

    private bool startedGrowing = false;
    private float currentRadius = 0f;

    void Update()
    {
        if (!startedGrowing)
        {
            // Remove destroyed/null mobs
            mobsToWatch.RemoveAll(mob => mob == null);

            if (mobsToWatch.Count == 0)
            {
                startedGrowing = true;
                currentRadius = 0f;
                shaderPosition.radius = currentRadius;
            }
        }
        else
        {
            if (currentRadius < targetRadius)
            {
                currentRadius += growSpeed * Time.deltaTime;
                shaderPosition.radius = Mathf.Min(currentRadius, targetRadius);
            }
        }
    }
}
