using UnityEngine;

public class LaunchpadTransform : MonoBehaviour
{
    public Transform targetPoint;
    public float arcHeight = 10f;
    public float launchDuration = 1.2f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PLAYER"))
        {
            StartCoroutine(LaunchPlayer(other.transform));
        }
    }

    private System.Collections.IEnumerator LaunchPlayer(Transform player)
    {
        Vector3 start = player.position;
        Vector3 end = targetPoint.position;
        float elapsed = 0f;

        while (elapsed < launchDuration)
        {
            float t = elapsed / launchDuration;
            Vector3 horizontal = Vector3.Lerp(start, end, t);
            //mitä pirua, tässä tapahtuu!? jotain gpt mystiikkä matikkaa
            float arc = 4 * arcHeight * t * (1 - t);
            Vector3 arcOffset = Vector3.up * arc;
            player.position = horizontal + arcOffset;
            elapsed += Time.deltaTime;
            yield return null;
        }

        player.position = end;
    }
}
