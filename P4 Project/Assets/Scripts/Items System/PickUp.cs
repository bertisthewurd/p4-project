using UnityEngine;

public class PickUp : Interactable
{
    [Header("Pickup Settings")]
    public Transform holdPoint;
    public float holdSmooth = 12f;
    public float maxHoldDistance = 3f;
    public Vector3 holdRotationOffset;
    private string originalPromptMessage;
    private Rigidbody rb;
    private static Rigidbody heldObject;
    private static bool holding = false;
    private Collider playerCollider;
    private Rigidbody playerRb;

    private bool inSlot = false;
    private FrameSlot currentSlot;
    public bool IsInSlot => inSlot;

    public event System.Action OnPlacedInSlot;
    public event System.Action OnFreedFromSlot;

    public static bool IsHolding => holding;

    public static PickUp CurrentlyHeld =>
        (holding && heldObject != null) ? heldObject.GetComponent<PickUp>() : null;

    public static void DropCurrent()
    {
        if (heldObject != null)
            heldObject.GetComponent<PickUp>().Drop();
    }

    protected void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.LogWarning($"Added missing Rigidbody to {gameObject.name}");
        }
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            playerCollider = player.GetComponent<Collider>();
            playerRb = player.GetComponent<Rigidbody>();
        }
    }

    protected void FixedUpdate()
    {
        if (holding && heldObject == rb && holdPoint != null)
        {
            float distance = Vector3.Distance(rb.position, holdPoint.position);
            if (distance > maxHoldDistance)
            {
                Drop();
                return;
            }

            Vector3 direction = holdPoint.position - rb.position;
            Vector3 targetVelocity = direction * holdSmooth;
            rb.linearVelocity = targetVelocity;

            Quaternion targetRot = holdPoint.rotation * Quaternion.Euler(holdRotationOffset);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * holdSmooth));
        }
    }

    protected override void Interact()
    {
        if (!holding)
        {
            Pickup();
        }
        else if (heldObject == rb)
        {
            Drop();
        }
    }

    private void Pickup()
    {
        if (holding || holdPoint == null) return;
        PickUpIntoHand();
        Debug.Log($"Picked up {gameObject.name}");
    }

    // Public so FrameSlot can reuse the same setup when transferring a frame
    // from a slot directly into the player's hand (e.g., during a swap).
    public void PickUpIntoHand()
    {
        if (holdPoint == null) return;
        if (holding && heldObject != rb)
        {
            Debug.LogWarning($"Cannot pick up {gameObject.name}: another object is already held.");
            return;
        }

        originalPromptMessage = promptMessage;
        promptMessage = "";

        heldObject = rb;
        holding = true;

        rb.isKinematic = false;
        rb.useGravity = false;
        rb.linearDamping = 12f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        SetCollisionWithPlayer(true);
    }

    // Snap into a FrameSlot. Clears any held state on this frame and locks it
    // in place. The frame's own colliders are disabled so the player's raycast
    // hits the slot's collider while the frame is parked.
    public void PlaceInSlot(FrameSlot slot)
    {
        if (slot == null) return;

        if (heldObject == rb)
        {
            heldObject = null;
            holding = false;
            promptMessage = originalPromptMessage;
            SetCollisionWithPlayer(false);
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = false;
        rb.isKinematic = true;

        Transform target = slot.snapPoint != null ? slot.snapPoint : slot.transform;
        transform.SetPositionAndRotation(target.position, target.rotation);

        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;

        inSlot = true;
        currentSlot = slot;
        OnPlacedInSlot?.Invoke();

        Debug.Log($"Placed {gameObject.name} in {slot.name}");
    }

    // Free this frame from its slot. Restores physics but does not put it in
    // the player's hand — call PickUpIntoHand() for that.
    public void RemoveFromSlot()
    {
        if (!inSlot) return;

        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = true;

        rb.isKinematic = false;
        rb.useGravity = true;

        inSlot = false;
        currentSlot = null;
        OnFreedFromSlot?.Invoke();

        Debug.Log($"Removed {gameObject.name} from slot");
    }

    private void SetCollisionWithPlayer(bool ignore)
    {
        if (playerCollider == null) return;

        Collider[] allColliders = rb.GetComponentsInChildren<Collider>();
        foreach (Collider col in allColliders)
        {
            Physics.IgnoreCollision(col, playerCollider, ignore);
        }
    }

    private void Drop()
    {
        if (heldObject == null) return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = true;
        rb.linearDamping = 1f;
        rb.constraints = RigidbodyConstraints.None;

        SetCollisionWithPlayer(false);

        promptMessage = originalPromptMessage;
        heldObject = null;
        holding = false;

        Debug.Log($"Dropped {gameObject.name}");
    }

    // Prevents the object from pushing the player upward (skateboard effect)
    // while still allowing normal horizontal collision when not held.
    private void OnCollisionStay(Collision collision)
    {
        if (holding && heldObject == rb) return;
        if (playerRb == null || collision.rigidbody != playerRb) return;

        foreach (ContactPoint contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                Vector3 v = playerRb.linearVelocity;
                if (v.y > 0f) v.y = 0f;
                playerRb.linearVelocity = v;
                break;
            }
        }
    }
}
