using System.Collections.Generic;
using UnityEngine;

public class FrameSystem : MonoBehaviour
{
    private static readonly int EffectIntensityId = Shader.PropertyToID("_EffectIntensity");

    private readonly List<Material> _frameMaterials = new();

    void Awake()
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            foreach (Material m in r.materials)
            {
                if (m != null && m.shader != null && m.shader.name.Contains("FrameImageEffect"))
                    _frameMaterials.Add(m);
            }
        }
    }

    public void SetEffectIntensity(float intensity)
    {
        foreach (Material m in _frameMaterials)
            m.SetFloat(EffectIntensityId, intensity);
    }
}
