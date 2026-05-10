using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FMODLoopEmitter : MonoBehaviour
{
    [SerializeField] public EventReference eventRef;
    [SerializeField] public string effectParameterName = "EffectLevel";
    [SerializeField] public bool playOnAwake = false;

    private EventInstance _instance;

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
        ReleaseInstance();
    }

    public void Play()
    {
        if (_instance.isValid()) ReleaseInstance();
        if (eventRef.IsNull) return;

        _instance = RuntimeManager.CreateInstance(eventRef);
        RuntimeManager.AttachInstanceToGameObject(_instance, transform);
        _instance.start();
    }

    public void Stop()
    {
        if (!_instance.isValid()) return;
        _instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        ReleaseInstance();
    }

    public void SetEffectLevel(float value)
    {
        if (!_instance.isValid()) return;
        _instance.setParameterByName(effectParameterName, Mathf.Clamp01(value));
    }

    public void SetVolume(float value)
    {
        if (!_instance.isValid()) return;
        _instance.setVolume(Mathf.Clamp01(value));
    }

    private void ReleaseInstance()
    {
        if (!_instance.isValid()) return;
        _instance.release();
        _instance = default;
    }
}
