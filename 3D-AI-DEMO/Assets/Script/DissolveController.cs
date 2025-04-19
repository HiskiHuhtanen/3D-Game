using System.Collections;
using UnityEngine;

public class DissolveController : MonoBehaviour
{
    public float dissolveSpeed = 1f;
    public float delayBeforeDissolve = 0.5f;
    public float delayBeforeDelete = 0.5f;
    public Material dissolveMat;
    private Material[] _originalBaseMaterial;
    private Material[] _dissolveMaterialInstance;
    private SkinnedMeshRenderer[] _renderer;
    private bool _isDissolving = false;

    private void Awake()
    {
        _renderer = GetComponentsInChildren<SkinnedMeshRenderer>();
        if (_renderer.Length == 0)
        {
            Debug.LogWarning("Eipä tullu mitään");
            return;
        }

        _originalBaseMaterial = new Material[_renderer.Length];
        _dissolveMaterialInstance = new Material[_renderer.Length];

        for (int i = 0; i < _renderer.Length; i++)
        {
            if (_renderer[i].materials.Length > 0)
            {
                _originalBaseMaterial[i] = _renderer[i].materials[0];
            }
        }

    }

    public void StartDissolve()
    {
        if (_isDissolving || _originalBaseMaterial == null || dissolveMat == null)
        {
            Debug.LogError("Dissolve failed: missing material or already dissolving.");
            return;
        }

        for (int i = 0; i < _renderer.Length; i++)
        {
            if (_originalBaseMaterial[i] != null)
            {
                var instance = new Material(dissolveMat);
                CopyTextures(_originalBaseMaterial[i], instance);
                instance.SetFloat("_DissolveAmount", -1f);
                _dissolveMaterialInstance[i] = instance;
                _renderer[i].materials = new Material[] { instance };
            }
        }

        StartCoroutine(DissolveRoutine());
    }

    private void CopyTextures(Material from, Material to)
    {
        Debug.Log(from.GetTexture("_BumpMap")); // Should not be null

        if (from.HasProperty("_BaseMap") && to.HasProperty("_BaseTex"))
            to.SetTexture("_BaseTex", from.GetTexture("_BaseMap"));
        if (from.HasProperty("_Color") && to.HasProperty("_Color"))
            to.SetColor("_Color", from.GetColor("_Color"));
        if (from.HasProperty("_BumpMap") && to.HasProperty("_BumpMap"))
            to.SetTexture("_BumpMap", from.GetTexture("_BumpMap"));
            to.EnableKeyword("_NORMALMAP");
    }

    private IEnumerator DissolveRoutine()
    {
        _isDissolving = true;
        yield return new WaitForSeconds(delayBeforeDissolve);

        float dissolveAmount = -1f;

        while (dissolveAmount < 1f)
        {
            dissolveAmount += Time.deltaTime * dissolveSpeed;
            foreach (var mat in _dissolveMaterialInstance)
            {
                if (mat != null)
                {
                    mat.SetFloat("_DissolveAmount", dissolveAmount);
                }
            }

            yield return null;
        }

        foreach (var mat in _dissolveMaterialInstance)
        {
            mat?.SetFloat("_DissolveAmount", 1f);
        }

        yield return new WaitForSeconds(delayBeforeDelete);
        Destroy(gameObject);
    }
}
