using UnityEngine;

public class FrameSlot : Interactable
{
    [Header("Slot Settings")]
    [Tooltip("Where the frame snaps to when placed. Defaults to this transform.")]
    public Transform snapPoint;

    [Header("Prompts")]
    public string placePrompt = "Press E to place frame";
    public string takePrompt  = "Press E to take frame";
    public string swapPrompt  = "Press E to swap frames";

    [Header("Debug")]
    public bool drawGizmo = true;
    public Color emptyColor  = new Color(0f, 1f, 0f, 0.25f);
    public Color filledColor = new Color(1f, 0.5f, 0f, 0.25f);

    private PickUp heldFrame;
    public bool IsEmpty => heldFrame == null;
    public PickUp HeldFrame => heldFrame;

    void Awake()
    {
        if (snapPoint == null) snapPoint = transform;
        promptMessage = "";
    }

    void Update()
    {
        // Keep the prompt accurate based on whether the player is holding a frame.
        bool playerHolding = PickUp.IsHolding;
        if (IsEmpty)
            promptMessage = playerHolding ? placePrompt : "";
        else
            promptMessage = playerHolding ? swapPrompt : takePrompt;
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
