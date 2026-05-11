using System.Collections;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODLoopEmitter : MonoBehaviour
{
    [SerializeField] public EventReference eventRef;
    [SerializeField] public string effectParameterName = "EffectLevel";
    [Tooltip("Tick if the effect parameter is a Global parameter in FMOD Studio (set on the Studio System). Untick if it's a Local parameter on the event itself.")]
    [SerializeField] public bool effectParameterIsGlobal = true;
    [SerializeField] public bool playOnAwake = false;

    [Header("Fades")]
    [Tooltip("Seconds to fade volume in when Play() is called.")]
    [SerializeField] public float fadeInDuration = 0.5f;
    [Tooltip("Seconds to fade volume out when Stop() is called.")]
    [SerializeField] public float fadeOutDuration = 0.5f;

    private EventInstance _instance;
    private Coroutine _fadeRoutine;
    private float _currentVolume = 1f;

    public bool IsPlaying
    {
        get
        {
            if (!_instance.isValid()) return false;
            _instance.getPlaybackState(out PLAYBACK_STATE state);
            return state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING;
        }
    }

    void Start()
    {
        if (playOnAwake) Play();
    }

    void OnDestroy()
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        ReleaseInstance();
    }

    public void Play()
    {
        if (eventRef.IsNull) return;
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(PlayWithFadeIn());
    }

    public void Stop()
    {
        if (!_instance.isValid()) return;
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(StopWithFadeOut());
    }

    public void SetEffectLevel(float value)
    {
        float clamped = Mathf.Clamp01(value);
        if (effectParameterIsGlobal)
        {
            RuntimeManager.StudioSystem.setParameterByName(effectParameterName, clamped);
            return;
        }
        if (!_instance.isValid()) return;
        _instance.setParameterByName(effectParameterName, clamped);
    }

    public void SetVolume(float value)
    {
        _currentVolume = Mathf.Clamp01(value);
        if (_instance.isValid()) _instance.setVolume(_currentVolume);
    }

    private IEnumerator PlayWithFadeIn()
    {
        if (!_instance.isValid())
        {
            _instance = RuntimeManager.CreateInstance(eventRef);
            RuntimeManager.AttachInstanceToGameObject(_instance, gameObject);
            _currentVolume = 0f;
            _instance.setVolume(0f);
            _instance.start();
        }
        yield return FadeTo(1f, fadeInDuration);
        _fadeRoutine = null;
    }

    private IEnumerator StopWithFadeOut()
    {
        yield return FadeTo(0f, fadeOutDuration);
        if (_instance.isValid())
        {
            _instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        ReleaseInstance();
        _fadeRoutine = null;
    }

    private IEnumerator FadeTo(float targetVolume, float duration)
    {
        duration = Mathf.Max(0.01f, duration);
        float from = _currentVolume;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            _currentVolume = Mathf.Lerp(from, targetVolume, Mathf.Clamp01(t / duration));
            if (_instance.isValid()) _instance.setVolume(_currentVolume);
            yield return null;
        }
        _currentVolume = targetVolume;
        if (_instance.isValid()) _instance.setVolume(_currentVolume);
    }

    private void ReleaseInstance()
    {
        if (!_instance.isValid()) return;
        _instance.release();
        _instance = default;
    }
}
