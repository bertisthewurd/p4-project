using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODSequentialPlayer : MonoBehaviour
{
    [Tooltip("Seconds the next clip takes to fade in while the previous one fades out. Set to 0 for hard cuts.")]
    [SerializeField] public float crossfadeDuration = 0.5f;
    [Tooltip("Fallback clip length (seconds) if FMOD can't report the event's natural length.")]
    [SerializeField] public float fallbackClipLength = 4f;
    [Tooltip("Played instead of the sequence when isSolved is true.")]
    [SerializeField] public EventReference storyRevealEvent;
    [Tooltip("Transform that sequential playback (and the story reveal) follows — typically the player's camera so the audio sounds 'in the player's ear'. Leave empty to play from this GameObject's position.")]
    [SerializeField] public Transform playerListener;
    [Tooltip("Volume multiplier applied to sequential clips. Values > 1 amplify the signal (FMOD allows this up to its internal headroom). The story reveal event is not affected.")]
    [SerializeField] public float sequenceVolume = 1.5f;

    // Populated at runtime by the controller in slot order.
    public List<FMODLoopEmitter> Emitters { get; set; } = new();

    private Coroutine _playRoutine;
    private EventInstance _storyInstance;
    private EventInstance _sequenceInstance;
    private EventInstance _previousInstance;

    public bool IsPlaying => _playRoutine != null;

    public void Play(bool isSolved, float effectLevel)
    {
        if (_playRoutine != null) StopCoroutine(_playRoutine);
        StopStory();
        StopSequenceInstances();
        _playRoutine = StartCoroutine(isSolved ? PlayStory() : PlaySequence(effectLevel));
    }

    public void Stop()
    {
        if (_playRoutine != null)
        {
            StopCoroutine(_playRoutine);
            _playRoutine = null;
        }
        StopSequenceInstances();
        StopStory();
    }

    void OnDestroy() => Stop();

    private IEnumerator PlaySequence(float effectLevel)
    {
        Transform anchor = playerListener != null ? playerListener : transform;
        float targetVolume = Mathf.Max(0f, sequenceVolume);

        // Apply the current effect level once — the parameter is global.
        if (Emitters.Count > 0 && Emitters[0] != null)
            Emitters[0].SetEffectLevel(effectLevel);

        float fadeDur = Mathf.Max(0.01f, crossfadeDuration);

        foreach (var emitter in Emitters)
        {
            if (emitter == null || emitter.eventRef.IsNull) continue;

            // Use the event's natural timeline length so each clip plays exactly once,
            // even though the event has a loop region (needed by the ambient behavior).
            float clipLength = GetEventLengthSeconds(emitter.eventRef);
            if (clipLength <= 0f) clipLength = fallbackClipLength;

            // Promote the currently-playing instance to "previous" so it fades out
            // while the new one fades in.
            _previousInstance = _sequenceInstance;

            _sequenceInstance = RuntimeManager.CreateInstance(emitter.eventRef);
            RuntimeManager.AttachInstanceToGameObject(_sequenceInstance, anchor.gameObject);
            _sequenceInstance.setVolume(0f);
            _sequenceInstance.start();

            // Crossfade — this is also the first `fadeDur` of this clip's natural playback.
            float t = 0f;
            while (t < fadeDur)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / fadeDur);
                if (_sequenceInstance.isValid()) _sequenceInstance.setVolume(p * targetVolume);
                if (_previousInstance.isValid()) _previousInstance.setVolume((1f - p) * targetVolume);
                yield return null;
            }
            if (_sequenceInstance.isValid()) _sequenceInstance.setVolume(targetVolume);
            ReleasePreviousInstance();

            // Sustain until `fadeDur` before the natural end. The next iteration will
            // start the next clip, which fades in while this one fades out — so this
            // clip plays for exactly `clipLength` seconds total and never loops.
            float sustain = clipLength - 2f * fadeDur;
            if (sustain > 0f) yield return new WaitForSeconds(sustain);
        }

        // Fade out the final clip (no next clip to crossfade with).
        if (_sequenceInstance.isValid())
        {
            float t = 0f;
            while (t < fadeDur)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / fadeDur);
                if (_sequenceInstance.isValid()) _sequenceInstance.setVolume((1f - p) * targetVolume);
                yield return null;
            }
            StopSequenceInstances();
        }

        _playRoutine = null;
    }

    private float GetEventLengthSeconds(EventReference eventRef)
    {
        if (eventRef.IsNull) return 0f;
        try
        {
            var desc = RuntimeManager.GetEventDescription(eventRef);
            desc.getLength(out int lengthMs);
            return lengthMs > 0 ? lengthMs / 1000f : 0f;
        }
        catch
        {
            return 0f;
        }
    }

    private IEnumerator PlayStory()
    {
        Transform anchor = playerListener != null ? playerListener : transform;

        if (!storyRevealEvent.IsNull)
        {
            _storyInstance = RuntimeManager.CreateInstance(storyRevealEvent);
            RuntimeManager.AttachInstanceToGameObject(_storyInstance, anchor.gameObject);
            _storyInstance.start();
        }
        _playRoutine = null;
        yield break;
    }

    private void StopStory()
    {
        if (!_storyInstance.isValid()) return;
        _storyInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _storyInstance.release();
        _storyInstance = default;
    }

    private void ReleasePreviousInstance()
    {
        if (!_previousInstance.isValid()) return;
        _previousInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        _previousInstance.release();
        _previousInstance = default;
    }

    private void StopSequenceInstances()
    {
        ReleasePreviousInstance();
        if (_sequenceInstance.isValid())
        {
            _sequenceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _sequenceInstance.release();
            _sequenceInstance = default;
        }
    }
}
