using UnityEngine;

public class FrameSlot : Interactable
{
    [Header("Slot Settings")]
    [Tooltip("Where the frame snaps to when placed. Defaults to this transform.")]
    public Transform snapPoint;
    [Tooltip("Frame prefab used to size the runtime highlight. Assign Assets/Prefabs/Frame.prefab.")]
    public GameObject framePrefab;

    [Header("Prompts")]
    public string placePrompt = "Press E to place frame";
    public string takePrompt  = "Press E to take frame";
    public string swapPrompt  = "Press E to swap frames";

    [Header("Debug")]
    public bool drawGizmo = true;
    public bool showRuntimeHighlight = true;
    [Tooltip("Size of the highlight box in snap point's local space.")]
    public Vector3 highlightSize = new Vector3(1f, 1.2f, 0.1f);
    [Tooltip("Position offset of the highlight box relative to the snap point.")]
    public Vector3 highlightOffset = new Vector3(0f, 0.6f, 0f);
    public Color emptyColor  = new Color(0f, 1f, 0f, 0.25f);
    public Color filledColor = new Color(1f, 0.5f, 0f, 0.25f);

    private PickUp heldFrame;

    private GameObject _highlightObj;
    private Renderer _highlightRenderer;
    private MaterialPropertyBlock _mpb;
    private string _colorProperty;
    private bool _isFocused;
    public bool IsEmpty => heldFrame == null;
    public PickUp HeldFrame => heldFrame;

    void Awake()
    {
        if (snapPoint == null) snapPoint = transform;
        promptMessage = "";
    }

    void Start()
    {
        if (showRuntimeHighlight) SetupHighlight();
    }

    private void SetupHighlight()
    {
        _highlightObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _highlightObj.name = "_SlotHighlight";
        _highlightObj.layer = LayerMask.NameToLayer("Ignore Raycast");

        var c = _highlightObj.GetComponent<Collider>();
        if (c != null) { c.enabled = false; Destroy(c); }

        Transform pivot = snapPoint != null ? snapPoint : transform;
        _highlightObj.transform.SetParent(pivot);
        _highlightObj.transform.localPosition = highlightOffset;
        _highlightObj.transform.localRotation = Quaternion.identity;
        _highlightObj.transform.localScale    = highlightSize;

        _highlightRenderer = _highlightObj.GetComponent<MeshRenderer>();
        _highlightRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _highlightRenderer.receiveShadows = false;
        _highlightRenderer.material = CreateHighlightMaterial();

        _mpb = new MaterialPropertyBlock();
        _highlightObj.SetActive(false);
    }

    private Material CreateHighlightMaterial()
    {
        Shader urp = Shader.Find("Universal Render Pipeline/Unlit");
        if (urp != null)
        {
            _colorProperty = "_BaseColor";
            var m = new Material(urp);
            m.SetFloat("_Surface", 1f);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.SetInt("_ZWrite", 0);
            m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            return m;
        }
        _colorProperty = "_Color";
        return new Material(Shader.Find("Sprites/Default"));
    }

    void Update()
    {
        // Keep the prompt accurate based on whether the player is holding a frame.
        bool playerHolding = PickUp.IsHolding;
        if (IsEmpty)
            promptMessage = playerHolding ? placePrompt : "";
        else
            promptMessage = playerHolding ? swapPrompt : takePrompt;

        // Refresh highlight color every frame while focused so it reacts
        // immediately when a frame is placed into or taken from the slot.
        if (_isFocused && _highlightObj != null && showRuntimeHighlight)
        {
            // Green when player is holding (place or swap); orange only when taking.
            Color highlightColor = (!IsEmpty && !PickUp.IsHolding) ? filledColor : emptyColor;
            _mpb.SetColor(_colorProperty, highlightColor);
            _highlightRenderer.SetPropertyBlock(_mpb);
        }
    }

    protected override void Interact()
    {
        PickUp current = PickUp.CurrentlyHeld;

        if (IsEmpty)
        {
            if (current == null) return;            // nothing to place
            current.PlaceInSlot(this);
            heldFrame = current;
        }
        else
        {
            PickUp oldFrame = heldFrame;

            // Free the old frame from the slot first so the slot is empty.
            oldFrame.RemoveFromSlot();
            heldFrame = null;

            if (current != null)
            {
                // SWAP: place the currently-held frame, take the old one into hand.
                current.PlaceInSlot(this);
                heldFrame = current;
                oldFrame.PickUpIntoHand();
            }
            else
            {
                // Just take the slot frame into the player's hand.
                oldFrame.PickUpIntoHand();
            }
        }
    }

    public override void OnFocusEnter()
    {
        _isFocused = true;
        if (_highlightObj != null && showRuntimeHighlight)
            _highlightObj.SetActive(true);
    }

    public override void OnFocusExit()
    {
        _isFocused = false;
        if (_highlightObj != null)
            _highlightObj.SetActive(false);
    }

    void OnDisable()
    {
        _isFocused = false;
        if (_highlightObj != null)
            _highlightObj.SetActive(false);
    }

    void OnDrawGizmos()
    {
        if (!drawGizmo) return;

        Color baseColor = IsEmpty ? emptyColor : filledColor;

        // Find the box collider whether it's on this GameObject or a child.
        BoxCollider box = GetComponentInChildren<BoxCollider>();
        if (box != null)
        {
            Matrix4x4 prev = Gizmos.matrix;
            // Use the collider's own transform so the gizmo lines up with the
            // collider regardless of which object in the hierarchy it sits on.
            Gizmos.matrix = box.transform.localToWorldMatrix;

            Gizmos.color = baseColor;
            Gizmos.DrawCube(box.center, box.size);

            Gizmos.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
            Gizmos.DrawWireCube(box.center, box.size);

            Gizmos.matrix = prev;
        }

        Transform sp = snapPoint != null ? snapPoint : transform;
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(sp.position, 0.05f);
        Gizmos.DrawLine(sp.position, sp.position + sp.forward * 0.2f);
    }
}
