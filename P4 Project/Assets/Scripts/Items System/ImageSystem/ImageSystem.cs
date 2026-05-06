using System.Collections.Generic;
using UnityEngine;

public class ImageSystem : MonoBehaviour
{
    [System.Serializable]
    public struct FrameSlotMapping
    {
        public FrameSlot slot;
        public PickUp    correctFrame;
    }

    public FrameSystem            frameSystem;
    public List<FrameSlotMapping> mappings = new();
    [Tooltip("Seconds the transition takes to settle. Higher = slower and smoother.")]
    public float                  transitionSmoothTime = 0.4f;
    [Tooltip("Values > 1 make the last correct placements have the most dramatic visual impact.")]
    [Range(1f, 4f)]
    public float                  intensityExponent = 2f;

    private float _targetIntensity  = 1f;
    private float _currentIntensity = 1f;
    private float _smoothVelocity   = 0f;

    void Start()
    {
        foreach (var mapping in mappings)
        {
            if (mapping.slot == null) continue;
            mapping.slot.OnFramePlaced  += _ => RecountAndUpdateTarget();
            mapping.slot.OnFrameRemoved += _ => RecountAndUpdateTarget();
            mapping.slot.OnFrameEjected += _ => RecountAndUpdateTarget();
        }

        RecountAndUpdateTarget();
        _currentIntensity = _targetIntensity;
        frameSystem?.SetEffectIntensity(ApplyCurve(_currentIntensity));
    }

    void Update()
    {
        if (Mathf.Approximately(_currentIntensity, _targetIntensity)) return;

        _currentIntensity = Mathf.SmoothDamp(_currentIntensity, _targetIntensity,
                                              ref _smoothVelocity, transitionSmoothTime);
        frameSystem?.SetEffectIntensity(ApplyCurve(_currentIntensity));
    }

    private float ApplyCurve(float t) => Mathf.Pow(t, 1f / intensityExponent);

    private void RecountAndUpdateTarget()
    {
        int correctCount = 0;
        foreach (var mapping in mappings)
        {
            if (mapping.slot != null &&
                mapping.slot.HeldFrame != null &&
                mapping.slot.HeldFrame == mapping.correctFrame)
            {
                correctCount++;
            }
        }

        int total = mappings.Count;
        if (total == 0) { _targetIntensity = 0f; return; }

        _targetIntensity = (total - correctCount) / (float)total;
    }
}
