using UnityEngine;

public class Spawner : MonoBehaviour
{

    public Transform[] spawnPoints;

    public GameObject meleeEnemy;
    public GameObject rangedEnemy;

    public float spawnCooldown = 2f;

    private void Start()
    {
        InvokeRepeating(nameof(SpawnEnemy), 0f, spawnCooldown);
    }

    void SpawnEnemy()
    {
        int randomIndex = Random.Range(0, spawnPoints.Length);
        float spawnChance = Random.Range(0f, 1f);
        Debug.Log("chance " + spawnChance);
        if (spawnChance < 0.6f)
        {
            Instantiate(rangedEnemy, spawnPoints[randomIndex].position, spawnPoints[randomIndex].rotation);
        } else {
            Instantiate(meleeEnemy, spawnPoints[randomIndex].position, spawnPoints[randomIndex].rotation);
        }
    }
}
