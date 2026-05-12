using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    public PortalTeleporter otherTeleport;
    private bool hasTeleported = false;

    private void OnTriggerEnter(Collider other)
    {
        hasTeleported = false; // Reset when entering the trigger
    }

    private void OnTriggerStay(Collider other)
    {
        PickUp held = PickUp.CurrentlyHeld;
        // Held object teleports together with the player — skip its independent
        // trigger. The frame's collider lives on a child, so walk up the parents.
        if (held != null && other.GetComponentInParent<PickUp>() == held) return;

        // Resolve to the Rigidbody root so we move the whole body, not a child
        // collider that would desync from its parent transform.
        Transform target = other.attachedRigidbody != null
            ? other.attachedRigidbody.transform
            : other.transform;

        float zPosition = transform.worldToLocalMatrix.MultiplyPoint3x4(target.position).z;
        if (zPosition >= 0) return;

        Teleport(target);

        if (held != null)
        {
            Teleport(held.transform);
            Rigidbody heldRb = held.GetComponent<Rigidbody>();
            if (heldRb != null)
            {
                heldRb.linearVelocity = Vector3.zero;
                heldRb.angularVelocity = Vector3.zero;
            }
        }
    }

    private void Teleport(Transform objectToTeleport)
    {
        Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint3x4(objectToTeleport.position);
        localPosition = new Vector3(-localPosition.x, localPosition.y, -localPosition.z);
        Vector3 newPosition = otherTeleport.transform.localToWorldMatrix.MultiplyPoint3x4(localPosition);

        Quaternion diffrence = otherTeleport.transform.rotation * Quaternion.Inverse(transform.rotation * Quaternion.Euler(0, 180, 0));
        Quaternion newRotation = diffrence * objectToTeleport.rotation;

        // Sync Rigidbody state so the next FixedUpdate reads the new pose;
        // rotating velocity preserves momentum through the portal so the player
        // doesn't drag back in along their old direction.
        Rigidbody rb = objectToTeleport.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = newPosition;
            rb.rotation = newRotation;
            rb.linearVelocity = diffrence * rb.linearVelocity;
            rb.angularVelocity = diffrence * rb.angularVelocity;
        }
        objectToTeleport.SetPositionAndRotation(newPosition, newRotation);
    }

}