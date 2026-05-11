using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class FramePuzzleAudioController : MonoBehaviour
{
    [SerializeField] private ImageSystem imageSystem;
    [SerializeField] private FrameSlotsManager slotsManager;
    [SerializeField] private FMODSequentialPlayer sequentialPlayer;
    [Tooltip("Played as a one-shot when a frame is placed in its correct slot.")]
    [SerializeField] private EventReference rightImageClickEvent;
    [Tooltip("Played as a one-shot when a frame is placed in an incorrect slot.")]
    [SerializeField] private EventReference wrongImageClickEvent;
    [Tooltip("Played when a shelf transitions from ghost (locked) to normal (unlocked) material.")]
    [SerializeField] private EventReference ghostShelfWooshEvent;
    [Tooltip("Played when a shelf transitions from normal (unlocked) to ghost (locked) material.")]
    [SerializeField] private EventReference ghostShelfWooshBackwardsEvent;

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
        imageSystem.OnFrameCorrectlyPlaced += OnFrameCorrectlyPlaced;
        imageSystem.OnFrameWronglyPlaced += OnFrameWronglyPlaced;
        _currentEffectLevel = imageSystem.CurrentIntensity;

        foreach (var slot in slotsManager.frameSlots)
        {
            if (slot == null) continue;
            slot.OnUnlockAnimationStarted += OnSlotUnlockAnimationStarted;
            slot.OnLockAnimationStarted += OnSlotLockAnimationStarted;
        }

        // Defer initial Play() to allow FMOD banks to finish loading.
        // Otherwise CreateInstance/start can silently no-op at scene boot.
        StartCoroutine(StartInitialEmittersDeferred());
    }

    private IEnumerator StartInitialEmittersDeferred()
    {
        yield return null;

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
        {
            imageSystem.OnIntensityChanged -= OnIntensityChanged;
            imageSystem.OnFrameCorrectlyPlaced -= OnFrameCorrectlyPlaced;
            imageSystem.OnFrameWronglyPlaced -= OnFrameWronglyPlaced;
        }
        if (slotsManager != null)
        {
            foreach (var slot in slotsManager.frameSlots)
            {
                if (slot == null) continue;
                slot.OnUnlockAnimationStarted -= OnSlotUnlockAnimationStarted;
                slot.OnLockAnimationStarted -= OnSlotLockAnimationStarted;
            }
        }
    }

    private void OnFrameCorrectlyPlaced(FrameSlot slot)
    {
        if (rightImageClickEvent.IsNull || slot == null) return;
        RuntimeManager.PlayOneShot(rightImageClickEvent, slot.transform.position);
    }

    private void OnFrameWronglyPlaced(FrameSlot slot)
    {
        if (wrongImageClickEvent.IsNull || slot == null) return;
        RuntimeManager.PlayOneShot(wrongImageClickEvent, slot.transform.position);
    }

    private void OnSlotUnlockAnimationStarted(FrameSlot slot)
    {
        if (ghostShelfWooshEvent.IsNull || slot == null) return;
        RuntimeManager.PlayOneShot(ghostShelfWooshEvent, slot.transform.position);
    }

    private void OnSlotLockAnimationStarted(FrameSlot slot)
    {
        if (ghostShelfWooshBackwardsEvent.IsNull || slot == null) return;
        RuntimeManager.PlayOneShot(ghostShelfWooshBackwardsEvent, slot.transform.position);
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
