using UnityEngine;

public class FireballMover : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private bool isReflected = false; // NEW
    private Transform dragonTarget; // NEW


    public void Initialize(Vector3 dir, float spd)
    {
        direction = dir;
        speed = spd;
        Destroy(gameObject, 7f); // Destroy after 5 seconds if missed
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

     public void Reflect(Vector3 newDirection, Transform dragon)
    {
        Debug.Log("miumau");
        direction = newDirection.normalized;
        isReflected = true;
        dragonTarget = dragon;
        
        // Rotate the fireball visuals toward the new direction
        transform.forward = direction; // << Add this line to rotate it!
    }
}
