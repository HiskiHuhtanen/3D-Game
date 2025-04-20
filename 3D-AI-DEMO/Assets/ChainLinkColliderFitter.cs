using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ChainLinkColliderFitter : MonoBehaviour
{
    [ContextMenu("Add Box Colliders To Chain Link")]
    void AddColliders()
    {
        var meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("No MeshRenderer found!");
            return;
        }

        Bounds bounds = meshRenderer.bounds;
        bounds.center = transform.InverseTransformPoint(bounds.center); // convert to local space

        for (int i = 0; i < 4; i++)
        {
            GameObject colObj = new GameObject("BoxCollider_" + i);
            colObj.transform.parent = transform;
            colObj.transform.localPosition = bounds.center;

            var collider = colObj.AddComponent<BoxCollider>();

            // Slightly adjusted sizes/positions per collider
            Vector3 size = bounds.size * 0.5f;
            switch (i)
            {
                case 0:
                    size.y *= 0.6f;
                    colObj.transform.localPosition += new Vector3(0, 0.1f, 0);
                    break;
                case 1:
                    size.x *= 0.6f;
                    colObj.transform.localPosition += new Vector3(0.1f, 0, 0);
                    break;
                case 2:
                    size.z *= 0.6f;
                    colObj.transform.localPosition += new Vector3(0, 0, 0.1f);
                    break;
                case 3:
                    size *= 0.4f;
                    break;
            }

            collider.size = size;
        }

        Debug.Log("Added 4 colliders to " + gameObject.name);
    }
}
