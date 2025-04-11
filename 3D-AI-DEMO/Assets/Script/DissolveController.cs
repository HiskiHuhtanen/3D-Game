using System.Collections;
using UnityEngine;

public class DissolveController : MonoBehaviour
{
    public float dissolveSpeed = 1f;
    public float delayBeforeDissolve = 0.5f;
    public Material dissolveMat; // The dissolve shader with the _BaseTex property

    private Material _originalBaseMaterial;
    private Material _dissolveMaterialInstance;
    private SkinnedMeshRenderer _renderer;
    private bool _isDissolving = false;

    private void Awake()
    {
        _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (_renderer != null && _renderer.materials.Length > 0)
        {
            _originalBaseMaterial = _renderer.materials[0];
        }
        else
        {
            Debug.LogWarning("SkinnedMeshRenderer not found or no materials on " + gameObject.name);
        }
    }

    public void StartDissolve()
    {
        if (_isDissolving || _originalBaseMaterial == null || dissolveMat == null)
        {
            Debug.LogError("Dissolve failed: missing material or already dissolving.");
            return;
        }

        _dissolveMaterialInstance = new Material(dissolveMat);
        CopyTextures(_originalBaseMaterial, _dissolveMaterialInstance);
        _dissolveMaterialInstance.SetFloat("_DissolveAmount", -1f);
        _renderer.materials = new Material[] { _dissolveMaterialInstance };

        StartCoroutine(DissolveRoutine());
    }

    private void CopyTextures(Material from, Material to)
    {
        if (from.HasProperty("_BaseMap") && to.HasProperty("_BaseTex"))
            to.SetTexture("_BaseTex", from.GetTexture("_BaseMap"));

        if (from.HasProperty("_EmissionMap") && to.HasProperty("_EmissionMap"))
            to.SetTexture("_EmissionMap", from.GetTexture("_EmissionMap"));

        if (from.HasProperty("_Color") && to.HasProperty("_Color"))
            to.SetColor("_Color", from.GetColor("_Color"));
        
    }

    private IEnumerator DissolveRoutine()
    {
        _isDissolving = true;

        yield return new WaitForSeconds(delayBeforeDissolve);

        float dissolveAmount = -1f;

        while (dissolveAmount < 1f)
        {
            dissolveAmount += Time.deltaTime * dissolveSpeed;
            if (_dissolveMaterialInstance != null)
            {
                _dissolveMaterialInstance.SetFloat("_DissolveAmount", dissolveAmount);  // Dissolve effect
            }

            yield return null;
        }

        _dissolveMaterialInstance?.SetFloat("_DissolveAmount", 1f);
        yield return new WaitForSeconds(0.5f);

        Destroy(gameObject); // Destroy after dissolving
    }

    public bool IsDissolving() => _isDissolving;
}
