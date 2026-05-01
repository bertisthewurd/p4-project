using System.Collections;
using UnityEngine;

public class FrameSlot : Interactable
{
    [Header("Slot Settings")]
    [Tooltip("Where the frame snaps to when placed. Defaults to this transform.")]
    public Transform snapPoint;

    [Header("Highlight")]
    [Tooltip("Drag the SlotHighlight child object here from the hierarchy.")]
    public GameObject slotHighlight;

    [Header("Lock")]
    [Tooltip("Renderer the ghost lock material is applied to while the slot is locked (typically the shelf renderer).")]
    public Renderer lockTargetRenderer;
    [Tooltip("Ghost material asset. A unique runtime instance is created per slot so fades don't conflict.")]
    public Material ghostMaterial;
    [Tooltip("Shader property animated for the fade. 0 = ghost fully visible, 1 = ghost fully faded.")]
    public string transparencyProperty = "_Transparency";
    [Tooltip("Seconds for the ghost effect to fade off when the slot unlocks.")]
    public float unlockFadeDuration = 1.5f;
    [Tooltip("Seconds for the ghost effect to fade in when the slot re-locks.")]
    public float lockFadeDuration = 0.5f;

    [Header("Eject")]
    [Tooltip("Force applied to frames when they're ejected (Impulse mode). Negative to reverse direction.")]
    public float ejectForce = 5f;

    [Header("Prompts")]
    public string placePrompt = "PRESS E TO PLACE FRAME";
    public string takePrompt  = "PRESS E TO TAKE FRAME";
    public string swapPrompt  = "PRESS E TO SWAP FRAMES";

    private PickUp heldFrame;
    private bool _isFocused;
    private Material[] _originalMaterials;
    private Material _ghostInstance;
    private int _transparencyId;
    private Coroutine _visualRoutine;

    public bool IsEmpty => heldFrame == null;
    public PickUp HeldFrame => heldFrame;
    public bool IsLocked { get; private set; }

    public event System.Action<FrameSlot> OnFramePlaced;
    public event System.Action<FrameSlot> OnFrameRemoved;

    void Awake()
    {
        if (snapPoint == null) snapPoint = transform;
        promptMessage = "";

        if (lockTargetRenderer != null)
            _originalMaterials = lockTargetRenderer.sharedMaterials;
        if (ghostMaterial != null)
            _ghostInstance = new Material(ghostMaterial);
        _transparencyId = Shader.PropertyToID(transparencyProperty);
    }

    void Start()
    {
        if (slotHighlight != null)
            slotHighlight.SetActive(false);
    }

    void OnDestroy()
    {
        if (_ghostInstance != null) Destroy(_ghostInstance);
    }

    void Update()
    {
        if (IsLocked)
        {
            promptMessage = "";
            if (slotHighlight != null)
                slotHighlight.SetActive(false);
            return;
        }

        bool playerHolding = PickUp.IsHolding;
        if (IsEmpty)
            promptMessage = playerHolding ? placePrompt : "";
        else
            promptMessage = playerHolding ? swapPrompt : takePrompt;

        if (slotHighlight != null)
            slotHighlight.SetActive(_isFocused && playerHolding);
    }

    protected override void Interact()
    {
        if (IsLocked) return;

        PickUp current = PickUp.CurrentlyHeld;

        if (IsEmpty)
        {
            if (current == null) return;
            current.PlaceInSlot(this);
            heldFrame = current;
            OnFramePlaced?.Invoke(this);
        }
        else
        {
            PickUp oldFrame = heldFrame;

            oldFrame.RemoveFromSlot();
            heldFrame = null;

            if (current != null)
            {
                current.PlaceInSlot(this);
                heldFrame = current;
                oldFrame.PickUpIntoHand();
            }
            else
            {
                oldFrame.PickUpIntoHand();
                OnFrameRemoved?.Invoke(this);
            }
        }
    }

    public override void OnFocusEnter()
    {
        _isFocused = true;
    }

    public override void OnFocusExit()
    {
        _isFocused = false;
        if (slotHighlight != null)
            slotHighlight.SetActive(false);
    }

    void OnDisable()
    {
        _isFocused = false;
        if (slotHighlight != null)
            slotHighlight.SetActive(false);
    }

    public void LockImmediate()
    {
        IsLocked = true;
        promptMessage = "";
        if (_visualRoutine != null) { StopCoroutine(_visualRoutine); _visualRoutine = null; }
        if (_ghostInstance != null)
        {
            _ghostInstance.SetFloat(_transparencyId, 0f);
            ApplyGhostMaterial();
        }
    }

    public void MarkLocked()
    {
        if (IsLocked) return;
        IsLocked = true;
        promptMessage = "";
    }

    public void Lock()
    {
        IsLocked = true;
        promptMessage = "";
        if (_ghostInstance == null || lockTargetRenderer == null) return;

        if (_visualRoutine != null) StopCoroutine(_visualRoutine);

        if (!IsGhostApplied())
        {
            _ghostInstance.SetFloat(_transparencyId, 1f);
            ApplyGhostMaterial();
        }
        float from = _ghostInstance.GetFloat(_transparencyId);
        _visualRoutine = StartCoroutine(AnimateTransparency(from, 0f, lockFadeDuration, false));
    }

    public void Unlock()
    {
        if (!IsLocked) return;
        IsLocked = false;
        if (_ghostInstance == null || lockTargetRenderer == null) return;
        if (_visualRoutine != null) StopCoroutine(_visualRoutine);
        if (!IsGhostApplied()) return;
        float from = _ghostInstance.GetFloat(_transparencyId);
        _visualRoutine = StartCoroutine(AnimateTransparency(from, 1f, unlockFadeDuration, true));
    }

    private bool IsGhostApplied()
    {
        if (lockTargetRenderer == null || _ghostInstance == null) return false;
        var mats = lockTargetRenderer.sharedMaterials;
        return mats.Length > 0 && mats[0] == _ghostInstance;
    }

    private void ApplyGhostMaterial()
    {
        if (lockTargetRenderer == null || _ghostInstance == null || _originalMaterials == null) return;
        var arr = new Material[_originalMaterials.Length];
        for (int i = 0; i < arr.Length; i++) arr[i] = _ghostInstance;
        lockTargetRenderer.sharedMaterials = arr;
    }

    private void RestoreOriginalMaterials()
    {
        if (lockTargetRenderer == null || _originalMaterials == null) return;
        lockTargetRenderer.sharedMaterials = _originalMaterials;
    }

    private IEnumerator AnimateTransparency(float from, float to, float duration, bool restoreAtEnd)
    {
        duration = Mathf.Max(0.01f, duration);
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            _ghostInstance.SetFloat(_transparencyId, Mathf.Lerp(from, to, p));
            yield return null;
        }
        _ghostInstance.SetFloat(_transparencyId, to);
        if (restoreAtEnd)
            RestoreOriginalMaterials();
        _visualRoutine = null;
    }

    public void EjectFrame()
    {
        if (IsEmpty) return;

        PickUp frame = heldFrame;
        frame.RemoveFromSlot();
        heldFrame = null;

        if (frame.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 ejectDirection = transform.forward;
            rb.AddForce(ejectDirection * ejectForce, ForceMode.Impulse);
        }
    }
}
