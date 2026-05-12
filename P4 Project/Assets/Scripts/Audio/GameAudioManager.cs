using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }

    [Header("FMOD Events")]
    [SerializeField] private EventReference calmMusicEvent;
    [SerializeField] private EventReference upbeatMusicEvent;
    [SerializeField] private EventReference insideHouseEvent;
    [SerializeField] private EventReference ministryOfficeEvent;

    // ---- Persistent Instances (always playing) ----
    private EventInstance calmMusicInstance;
    private EventInstance upbeatMusicInstance;

    // ---- On-Demand Instances (created on first entry) ----
    private EventInstance insideHouseInstance;
    private EventInstance ministryOfficeInstance;
    private bool insideHouseStarted = false;
    private bool ministryOfficeStarted = false;

    // ---- Ministry Office Priority Flag ----
    private bool inMinistryOffice = false;

    // ---- Volume Fading ----
    private float calmVolumeCurrent = 1f,            calmVolumeTarget = 1f;
    private float upbeatVolumeCurrent = 0f,          upbeatVolumeTarget = 0f;
    private float insideHouseVolumeCurrent = 0f,     insideHouseVolumeTarget = 0f;
    private float ministryOfficeVolumeCurrent = 0f,  ministryOfficeVolumeTarget = 0f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeSpeed = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        calmMusicInstance   = RuntimeManager.CreateInstance(calmMusicEvent);
        upbeatMusicInstance = RuntimeManager.CreateInstance(upbeatMusicEvent);

        calmMusicInstance.setVolume(1f);
        upbeatMusicInstance.setVolume(0f);

        calmMusicInstance.start();
        upbeatMusicInstance.start();
    }

    private void Update()
    {
        calmVolumeCurrent = Mathf.MoveTowards(calmVolumeCurrent, calmVolumeTarget, fadeSpeed * Time.deltaTime);
        upbeatVolumeCurrent = Mathf.MoveTowards(upbeatVolumeCurrent, upbeatVolumeTarget, fadeSpeed * Time.deltaTime);

        calmMusicInstance.setVolume(calmVolumeCurrent);
        upbeatMusicInstance.setVolume(upbeatVolumeCurrent);

        if (insideHouseStarted)
        {
            insideHouseVolumeCurrent = Mathf.MoveTowards(insideHouseVolumeCurrent, insideHouseVolumeTarget, fadeSpeed * Time.deltaTime);
            insideHouseInstance.setVolume(insideHouseVolumeCurrent);
        }

        if (ministryOfficeStarted)
        {
            ministryOfficeVolumeCurrent = Mathf.MoveTowards(ministryOfficeVolumeCurrent, ministryOfficeVolumeTarget, fadeSpeed * Time.deltaTime);
            ministryOfficeInstance.setVolume(ministryOfficeVolumeCurrent);
        }
    }

    private void OnDestroy()
    {
        calmMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        calmMusicInstance.release();

        upbeatMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        upbeatMusicInstance.release();

        if (insideHouseStarted)
        {
            insideHouseInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            insideHouseInstance.release();
        }

        if (ministryOfficeStarted)
        {
            ministryOfficeInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            ministryOfficeInstance.release();
        }
    }

    // ==========================
    // Public Methods
    // ==========================

    public void SetNormalMusic(float completionPercent)
    {
        if (inMinistryOffice) return;

        insideHouseVolumeTarget = 0f;
        ministryOfficeVolumeTarget = 0f;

        if (completionPercent >= 50f)
        {
            calmVolumeTarget = 0f;
            upbeatVolumeTarget = 1f;
        }
        else
        {
            calmVolumeTarget = 1f;
            upbeatVolumeTarget = 0f;
        }
    }

    public void SetInsideHouseMusic()
    {
        if (inMinistryOffice) return;

        if (!insideHouseStarted)
        {
            insideHouseInstance = RuntimeManager.CreateInstance(insideHouseEvent);
            insideHouseInstance.setVolume(0f);
            insideHouseInstance.start();
            insideHouseStarted = true;
        }

        calmVolumeTarget = 0f;
        upbeatVolumeTarget = 0f;
        insideHouseVolumeTarget = 1f;
        ministryOfficeVolumeTarget = 0f;
    }

    public void SetMinistryOfficeMusic()
    {
        inMinistryOffice = true;

        if (!ministryOfficeStarted)
        {
            ministryOfficeInstance = RuntimeManager.CreateInstance(ministryOfficeEvent);
            ministryOfficeInstance.setVolume(0f);
            ministryOfficeInstance.start();
            ministryOfficeStarted = true;
        }

        calmVolumeTarget = 0f;
        upbeatVolumeTarget = 0f;
        insideHouseVolumeTarget = 0f;
        ministryOfficeVolumeTarget = 1f;
    }

    public void ExitMinistryOffice(float completionPercent)
    {
        inMinistryOffice = false;
        SetNormalMusic(completionPercent);
    }
}