using UnityEngine;

public class PickUp : Interactable
{
    [Header("Pickup Settings")]
    public Transform holdPoint;
    public float holdSmooth = 12f;
    public float dropDistance = 0.8f;  
    public float maxHoldDistance = 3f;
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

        Quaternion targetRot = holdPoint.rotation;
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
            Physics.IgnoreCollision(playerCollider, objectCol, true);

        Debug.Log($"Picked up {gameObject.name}");
    }

    private void Drop()
    {
        if (heldObject == null) return;

        rb.useGravity = true;
        rb.linearDamping = 1f;
        rb.constraints = RigidbodyConstraints.None;

        rb.position = holdPoint.position + holdPoint.forward * dropDistance;

        if (playerCollider && rb.TryGetComponent(out Collider objectCol))
            Physics.IgnoreCollision(playerCollider, objectCol, false);

        // Restore prompt
        promptMessage = originalPromptMessage;

        heldObject = null;
        holding = false;

        Debug.Log($"Dropped {gameObject.name}");
    }
}
