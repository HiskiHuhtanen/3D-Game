using System.Collections;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    public Transform player; // Pelaaja, käytetään myöhemmin
    public Transform[] pathPoints; // Polkupisteet (puun ympärillä tai ylhäällä)
    public ParticleSystem passiveFire; // Suussa oleva jatkuva tuli
    public ParticleSystem attackFire; // Syöksyvä tuli hyökkäyksessä
    public Animator animator; // Lohikäärmeen Animator

    private int currentPathIndex = 0;
    private bool isAttacking = false;
    private bool isTopPhase = false; // Onko pelaaja ylätasanteella?

    void Start()
    {
        // Käynnistetään passiivinen tuli (joka aina palaa)
        passiveFire.Play();
    }

    void Update()
    {
        // Jos lohikäärme ei hyökkää, se liikkuu pisteeltä toiselle
        if (!isAttacking)
        {
            MoveAlongPath();
        }

        // Jos ei olla ylävaiheessa, tehdään satunnaisesti hyökkäys
        if (!isTopPhase && !isAttacking && Random.Range(0f, 1f) < 0.002f)
        {
            StartCoroutine(AttackSequence());
        }
    }

    void MoveAlongPath()
    {
        Transform target = pathPoints[currentPathIndex];

        // Liikutaan kohti seuraavaa pistettä
        float speed = 2f;
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Katsotaan seuraavaan pisteeseen
        Vector3 direction = target.position - transform.position;
        if (direction != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 3f * Time.deltaTime);

        // Kun ollaan tarpeeksi lähellä pistettä, siirrytään seuraavaan
        if (Vector3.Distance(transform.position, target.position) < 0.5f)
        {
            currentPathIndex = (currentPathIndex + 1) % pathPoints.Length;
        }

        animator.Play("Slither"); // Ajetaan kiemurtelu-animaatio
    }

    IEnumerator AttackSequence()
    {
        isAttacking = true;

        animator.Play("Attack"); // Ajetaan hyökkäysanimaatio
        attackFire.Play();       // Käynnistetään tulisuihku

        yield return new WaitForSeconds(2f); // Odotetaan animaation ajan

        attackFire.Stop();       // Lopetetaan tuli
        isAttacking = false;     // Palataan liikkeeseen
    }

    public void SwitchToTopPath(Transform[] newPath)
    {
        // Vaihdetaan polku, kun pelaaja saapuu ylätasanteelle
        pathPoints = newPath;
        currentPathIndex = 0;
        isTopPhase = true;
    }
}
