using System.Collections;
using UnityEngine;

public class PlayButton : Interactable
{
    [SerializeField] private FramePuzzleAudioController audioController;
    [SerializeField] private FMODSequentialPlayer sequentialPlayer;
    [SerializeField] private Vector3 pressOffset = new Vector3(0f, 0f, -0.05f);
    [SerializeField] private float pressDuration = 0.08f;
    [SerializeField] private float releaseDuration = 0.15f;
    [SerializeField] private string idlePrompt = "PRESS E TO PLAY";

    private Vector3 _restLocalPos;
    private bool _isPressed;
    private Coroutine _animRoutine;

    void Awake()
    {
        _restLocalPos = transform.localPosition;
        promptMessage = idlePrompt;
    }

    void Update()
    {
        bool playing = sequentialPlayer != null && sequentialPlayer.IsPlaying;

        if (_isPressed && !playing)
        {
            _isPressed = false;
            StartAnim(AnimateTo(_restLocalPos, releaseDuration));
        }

        if (_isPressed || _animRoutine != null || PickUp.IsHolding)
            promptMessage = "";
        else
            promptMessage = idlePrompt;
    }

    protected override void Interact()
    {
        if (_isPressed) return;
        if (_animRoutine != null) return;
        if (PickUp.IsHolding) return;
        if (audioController == null) return;

        _isPressed = true;
        StartAnim(AnimateTo(_restLocalPos + pressOffset, pressDuration));
        audioController.OnPlayButtonPressed();
    }

    private void StartAnim(IEnumerator routine)
    {
        if (_animRoutine != null) StopCoroutine(_animRoutine);
        _animRoutine = StartCoroutine(routine);
    }

    private IEnumerator AnimateTo(Vector3 target, float duration)
    {
        Vector3 from = transform.localPosition;
        duration = Mathf.Max(0.01f, duration);
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(from, target, Mathf.Clamp01(t / duration));
            yield return null;
        }
        transform.localPosition = target;
        _animRoutine = null;
    }
}
