using System.Collections;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    public Transform[] pathPoints; // Polkupisteet (puun ymp‰rill‰ tai ylh‰‰ll‰)
    private int currentPathIndex = 0;

    void Start()
    {
    }

    void Update()
    {
        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        Transform target = pathPoints[currentPathIndex];

        // Liikutaan kohti seuraavaa pistett‰
        float speed = 100f;
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Katsotaan seuraavaan pisteeseen
        Vector3 direction = target.position - transform.position;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 3f * Time.deltaTime);

        // Kun ollaan tarpeeksi l‰hell‰ pistett‰, siirryt‰‰n seuraavaan
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            currentPathIndex = (currentPathIndex + 1) % pathPoints.Length;
        }
    }
}