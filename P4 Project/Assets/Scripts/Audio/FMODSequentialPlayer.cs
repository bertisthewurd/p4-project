using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODSequentialPlayer : MonoBehaviour
{
    [Tooltip("Seconds each clip plays before moving to the next one.")]
    [SerializeField] public float clipDuration = 4f;
    [Tooltip("Played instead of the sequence when isSolved is true.")]
    [SerializeField] public EventReference storyRevealEvent;

    // Populated at runtime by the controller in slot order.
    public List<FMODLoopEmitter> Emitters { get; set; } = new();

    private Coroutine _playRoutine;
    private EventInstance _storyInstance;

    public bool IsPlaying => _playRoutine != null;

    public void Play(bool isSolved, float effectLevel)
    {
        if (_playRoutine != null) StopCoroutine(_playRoutine);
        StopStory();
        _playRoutine = StartCoroutine(isSolved ? PlayStory() : PlaySequence(effectLevel));
    }

    public void Stop()
    {
        if (_playRoutine != null)
        {
            StopCoroutine(_playRoutine);
            _playRoutine = null;
        }
        foreach (var emitter in Emitters)
        {
            if (emitter != null) emitter.Stop();
        }
        StopStory();
    }

    void OnDestroy() => Stop();

    private IEnumerator PlaySequence(float effectLevel)
    {
        foreach (var emitter in Emitters)
        {
            if (emitter == null) continue;
            emitter.Play();
            emitter.SetEffectLevel(effectLevel);
            yield return new WaitForSeconds(clipDuration);
            emitter.Stop();
        }
        _playRoutine = null;
    }

    private IEnumerator PlayStory()
    {
        if (!storyRevealEvent.IsNull)
        {
            _storyInstance = RuntimeManager.CreateInstance(storyRevealEvent);
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
}
