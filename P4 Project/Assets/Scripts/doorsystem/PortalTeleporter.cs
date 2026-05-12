using UnityEngine;

public class PortalTeleporter : MonoBehaviour
{
    public PortalTeleporter otherTeleport;
    private bool hasTeleported = false;

    private void OnTriggerEnter(Collider other)
    {
        hasTeleported = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (hasTeleported) return;

        PickUp held = PickUp.CurrentlyHeld;
        if (held != null && other.GetComponentInParent<PickUp>() == held) return;

        // Resolve root transform — CharacterController has no Rigidbody,
        // so walk up to find CC or fall back to Rigidbody root.
        CharacterController cc = other.GetComponentInParent<CharacterController>();
        Rigidbody rb = other.attachedRigidbody;

        Transform target = cc != null ? cc.transform
                         : rb != null ? rb.transform
                         : other.transform;

        float zPosition = transform.worldToLocalMatrix.MultiplyPoint3x4(target.position).z;
        if (zPosition >= 0) return;

        hasTeleported = true;

        TeleportCharacterOrRigidbody(target, cc, rb);

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

    private void TeleportCharacterOrRigidbody(Transform target, CharacterController cc, Rigidbody rb)
    {
        Vector3 localPos = transform.worldToLocalMatrix.MultiplyPoint3x4(target.position);
        localPos = new Vector3(-localPos.x, localPos.y, -localPos.z);
        Vector3 newPosition = otherTeleport.transform.localToWorldMatrix.MultiplyPoint3x4(localPos);

        Quaternion difference = otherTeleport.transform.rotation
                              * Quaternion.Inverse(transform.rotation * Quaternion.Euler(0, 180, 0));
        Quaternion newRotation = difference * target.rotation;

        if (cc != null)
        {
            // Must disable CC to move it — otherwise Unity snaps it back.
            cc.enabled = false;
            target.SetPositionAndRotation(newPosition, newRotation);
            cc.enabled = true;

            // Rotate the player's velocity if you're tracking it externally.
            // If your movement script exposes a velocity field, rotate it here:
            // myMovement.velocity = difference * myMovement.velocity;
        }
        else
        {
            Teleport(target); // existing Rigidbody path
        }
    }

    private void Teleport(Transform objectToTeleport)
    {
        Vector3 localPos = transform.worldToLocalMatrix.MultiplyPoint3x4(objectToTeleport.position);
        localPos = new Vector3(-localPos.x, localPos.y, -localPos.z);
        Vector3 newPosition = otherTeleport.transform.localToWorldMatrix.MultiplyPoint3x4(localPos);

        Quaternion difference = otherTeleport.transform.rotation
                              * Quaternion.Inverse(transform.rotation * Quaternion.Euler(0, 180, 0));
        Quaternion newRotation = difference * objectToTeleport.rotation;

        Rigidbody rb = objectToTeleport.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.position = newPosition;
            rb.rotation = newRotation;
            rb.linearVelocity = difference * rb.linearVelocity;
            rb.angularVelocity = difference * rb.angularVelocity;
        }

        objectToTeleport.SetPositionAndRotation(newPosition, newRotation);
    }
}