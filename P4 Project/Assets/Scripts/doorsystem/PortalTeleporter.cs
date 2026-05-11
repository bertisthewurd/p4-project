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
        if (hasTeleported) return;

        Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint3x4(other.transform.position);

        // Check both z and x axes so all entry directions are caught
        if (localPosition.z < 0)
        {
            hasTeleported = true;
            Teleport(other.transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        hasTeleported = false; // Allow teleport again after exiting
    }

    private void Teleport(Transform objectToTeleport)
    {
        Debug.Log("teleport");

        Vector3 localPosition = transform.worldToLocalMatrix.MultiplyPoint3x4(objectToTeleport.position);
        localPosition = new Vector3(-localPosition.x, localPosition.y, -localPosition.z);
        objectToTeleport.position = otherTeleport.transform.localToWorldMatrix.MultiplyPoint3x4(localPosition);

        Quaternion difference = otherTeleport.transform.rotation * Quaternion.Inverse(transform.rotation * Quaternion.Euler(0, 180, 0));
        objectToTeleport.rotation = difference * objectToTeleport.rotation;
    }
}