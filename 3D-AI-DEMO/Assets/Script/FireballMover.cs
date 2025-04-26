using UnityEngine;

public class FireballMover : MonoBehaviour
{
    private Vector3 direction;
    private float speed;

    public void Initialize(Vector3 dir, float spd)
    {
        direction = dir;
        speed = spd;
        Destroy(gameObject, 5f); // Destroy after 5 seconds if missed
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}
