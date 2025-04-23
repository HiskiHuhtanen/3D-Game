using System.Collections;
using UnityEngine;

public class FallScript : MonoBehaviour
{
    public float fallThreshold = -20f;
    public Vector3 respawnOffset = Vector3.up * 3f;
    private Vector3 lastSafePosition;

    void Update()
    {
        if (IsGrounded())
        {
            lastSafePosition = transform.position;
        }

        if (transform.position.y < fallThreshold)
        {
            StartCoroutine(TeleportNextFrame());
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);
    }
    IEnumerator TeleportNextFrame()
    {
    yield return null; 
    transform.position = lastSafePosition + respawnOffset;
    }
}
