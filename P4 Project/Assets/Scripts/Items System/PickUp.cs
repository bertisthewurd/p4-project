using UnityEngine;

public class PickUp : Interactable
{
    [Header("Pickup Settings")]
    public Transform holdPoint;
    public float holdSmooth = 12f;
    public float dropDistance = 0.8f;  
    public float maxHoldDistance = 3f;
    public Vector3 holdRotationOffset;
    private string originalPromptMessage;
    private Rigidbody rb;
    private static Rigidbody heldObject;
    private static bool holding = false;
    private Collider playerCollider;    

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
            playerCollider = player.GetComponent<Collider>();
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

        originalPromptMessage = promptMessage;
        promptMessage = "";

        heldObject = rb;
        holding = true;

        rb.useGravity = false;
        rb.linearDamping = 12f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (playerCollider && rb.TryGetComponent(out Collider objectCol))
            SetCollisionWithPlayer(true);

        Debug.Log($"Picked up {gameObject.name}");
    }

    private void SetCollisionWithPlayer(bool ignore)
{
    if (playerCollider == null) return;
    
    // Get all colliders including children
    Collider[] allColliders = rb.GetComponentsInChildren<Collider>();
    foreach (Collider col in allColliders)
    {
        Physics.IgnoreCollision(col, playerCollider, ignore);
    }
}

    private void Drop()
    {
        if (heldObject == null) return;

        rb.useGravity = true;
        rb.linearDamping = 1f;
        rb.constraints = RigidbodyConstraints.None;

        Vector3 dropPos = holdPoint.position + holdPoint.forward * dropDistance;
        Collider objCol = rb.GetComponent<Collider>();
        if (objCol != null)
        {
            float radius = objCol.bounds.extents.magnitude;
            // Check if drop position is occupied
            if (Physics.CheckSphere(dropPos, radius, ~LayerMask.GetMask("Player")))
            {
                // Occupied, find a safe position
                RaycastHit hit;
                if (Physics.Raycast(holdPoint.position, holdPoint.forward, out hit, dropDistance * 2f))
                {
                    // Place just before the hit surface
                    dropPos = hit.point - holdPoint.forward * (radius + 0.1f);
                }
                else
                {
                    // No forward obstacle, check for ground below drop position
                    if (Physics.Raycast(dropPos, Vector3.down, out hit, 10f))
                    {
                        dropPos = hit.point + Vector3.up * (objCol.bounds.extents.y + 0.1f);
                    }
                    else
                    {
                        // No ground, fallback to hold point position
                        dropPos = holdPoint.position;
                    }
                }
            }
        }
        rb.position = dropPos;

        // Restore prompt
        promptMessage = originalPromptMessage;

        heldObject = null;
        holding = false;

        Debug.Log($"Dropped {gameObject.name}");
    }
}
