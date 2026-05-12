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

    public event System.Action<float> OnIntensityChanged;
    public event System.Action<FrameSlot> OnFrameCorrectlyPlaced;
    public event System.Action<FrameSlot> OnFrameWronglyPlaced;
    public float CurrentIntensity => _currentIntensity;
    public bool IsSolved => Mathf.Approximately(_targetIntensity, 0f);

    private readonly HashSet<FrameSlot> _correctSlots = new();
    private readonly HashSet<FrameSlot> _wrongSlots = new();

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
        if (frameSystem != null) frameSystem.SetEffectIntensity(ApplyCurve(_currentIntensity));
    }

    void Update()
    {
        if (Mathf.Approximately(_currentIntensity, _targetIntensity)) return;

        _currentIntensity = Mathf.SmoothDamp(_currentIntensity, _targetIntensity,
                                              ref _smoothVelocity, transitionSmoothTime);
        if (frameSystem != null) frameSystem.SetEffectIntensity(ApplyCurve(_currentIntensity));
        OnIntensityChanged?.Invoke(_currentIntensity);
    }

    private float ApplyCurve(float t) => Mathf.Pow(t, 1f / intensityExponent);

    private void RecountAndUpdateTarget()
    {
        int correctCount = 0;
        foreach (var mapping in mappings)
        {
            if (mapping.slot == null) continue;

            bool hasFrame = mapping.slot.HeldFrame != null;
            bool isCorrect = hasFrame && mapping.slot.HeldFrame == mapping.correctFrame;

            if (isCorrect)
            {
                correctCount++;
                _wrongSlots.Remove(mapping.slot);
                if (_correctSlots.Add(mapping.slot))
                    OnFrameCorrectlyPlaced?.Invoke(mapping.slot);
            }
            else if (hasFrame)
            {
                _correctSlots.Remove(mapping.slot);
                if (_wrongSlots.Add(mapping.slot))
                    OnFrameWronglyPlaced?.Invoke(mapping.slot);
            }
            else
            {
                _correctSlots.Remove(mapping.slot);
                _wrongSlots.Remove(mapping.slot);
            }
        }

        int total = mappings.Count;
        if (total == 0) { _targetIntensity = 0f; return; }

        _targetIntensity = (total - correctCount) / (float)total;
    }
}
