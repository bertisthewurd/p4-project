using System.Collections.Generic;
using UnityEngine;

public class FramePuzzleAudioController : MonoBehaviour
{
    [SerializeField] private ImageSystem imageSystem;
    [SerializeField] private FrameSlotsManager slotsManager;
    [SerializeField] private FMODSequentialPlayer sequentialPlayer;

    private float _currentEffectLevel = 1f;
    private readonly Dictionary<PickUp, FMODLoopEmitter> _pickUpEmitters = new();

    void Start()
    {
        // Cache all PickUp objects that have an emitter
        var allPickUps = FindObjectsByType<PickUp>(FindObjectsSortMode.None);
        foreach (var pickUp in allPickUps)
        {
            var emitter = pickUp.GetComponent<FMODLoopEmitter>();
            if (emitter == null) continue;

            _pickUpEmitters[pickUp] = emitter;
            pickUp.OnPlacedInSlot += () => OnPickUpPlaced(emitter);
            pickUp.OnFreedFromSlot += () => OnPickUpFreed(pickUp, emitter);
        }

        imageSystem.OnIntensityChanged += OnIntensityChanged;
        _currentEffectLevel = imageSystem.CurrentIntensity;

        // Start emitters for frames not currently in a slot
        foreach (var (pickUp, emitter) in _pickUpEmitters)
        {
            if (pickUp.IsInSlot)
                emitter.Stop();
            else
            {
                emitter.Play();
                emitter.SetEffectLevel(_currentEffectLevel);
            }
        }
    }

    void OnDestroy()
    {
        if (imageSystem != null)
            imageSystem.OnIntensityChanged -= OnIntensityChanged;
    }

    // Called by a UI Button's OnClick event.
    public void OnPlayButtonPressed()
    {
        RebuildSequentialEmitters();
        sequentialPlayer.Play(imageSystem.IsSolved, _currentEffectLevel);
    }

    private void OnIntensityChanged(float intensity)
    {
        _currentEffectLevel = intensity;
        foreach (var (_, emitter) in _pickUpEmitters)
        {
            if (emitter.IsPlaying)
                emitter.SetEffectLevel(intensity);
        }
    }

    private void OnPickUpPlaced(FMODLoopEmitter emitter)
    {
        emitter.Stop();
    }

    private void OnPickUpFreed(PickUp pickUp, FMODLoopEmitter emitter)
    {
        emitter.Play();
        emitter.SetEffectLevel(_currentEffectLevel);
    }

    private void RebuildSequentialEmitters()
    {
        sequentialPlayer.Emitters.Clear();
        foreach (var slot in slotsManager.frameSlots)
        {
            if (slot == null || slot.IsEmpty) continue;
            if (_pickUpEmitters.TryGetValue(slot.HeldFrame, out var emitter))
                sequentialPlayer.Emitters.Add(emitter);
        }
    }
}
