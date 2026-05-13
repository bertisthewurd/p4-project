using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    public Camera cam;
    [SerializeField]
    private float distance = 3f;
    [SerializeField]
    private PlayerUI playerUI;
    private Interactable _currentFocus;
    void Start()
    {
        //cam = GetComponent<Camera>();
        playerUI = GetComponent<PlayerUI>();
    }

    void Update()
    {
        playerUI.UpdateText(string.Empty);

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * distance);

        // Cast against everything, then walk the hits front-to-back so we can
        // see past the held frame (which sits between camera and the world)
        // and past empty slots / other "transparent" trigger volumes.
        PickUp held = PickUp.CurrentlyHeld;
        Transform camRoot = cam != null ? cam.transform.root : null;
        RaycastHit[] hits = Physics.SphereCastAll(ray, 0.05f, distance);

        Interactable hit = null;
        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            foreach (RaycastHit h in hits)
            {
                // Skip the player's own hierarchy (camera may overlap the player capsule).
                if (camRoot != null && h.collider.transform.IsChildOf(camRoot))
                    continue;
                // Skip the currently-held frame.
                if (held != null && h.collider.transform.IsChildOf(held.transform))
                    continue;

                // Walk up the hierarchy — the Interactable may live on a parent
                // (e.g. a FrameSlot whose BoxCollider is on a Cube child).
                Interactable candidate = h.collider.GetComponentInParent<Interactable>();
                if (candidate != null)
                {
                    // Treat empty-prompt interactables (e.g. an empty FrameSlot when
                    // we're not holding anything) as "nothing to do here, look past".
                    if (string.IsNullOrEmpty(candidate.promptMessage))
                        continue;

                    hit = candidate;
                    break;
                }

                // Solid (non-trigger) hit without an Interactable: this is a wall or
                // similar — it should genuinely block our line of sight.
                if (!h.collider.isTrigger)
                    break;

                // Trigger without an Interactable (logic volume, etc.): keep looking.
            }
        }

        // Notify focus changes so interactables can show/hide highlights.
        if (hit != _currentFocus)
        {
            _currentFocus?.OnFocusExit();
            _currentFocus = hit;
            _currentFocus?.OnFocusEnter();
        }

        if (PickUp.IsHolding)
        {
            // While holding, only FrameSlot interactables are valid targets:
            // place into an empty slot, or swap with a filled one.
            if (hit is FrameSlot slot)
            {
                playerUI.UpdateText(slot.promptMessage);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    if (slot.IsEmpty)
                        InteractionTracker.Log("place");
                    else
                        InteractionTracker.Log("swap");
                    slot.BaseInteract();
                }
                return;
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                InteractionTracker.Log("putdown");
                PickUp.DropCurrent();
            }
            return;
        }

        if (hit != null)
        {
            playerUI.UpdateText(hit.promptMessage);
            if (Input.GetKeyDown(KeyCode.E))
            {
                InteractionTracker.Log("pickup");
                hit.BaseInteract();
            }
        }
    }

    void OnDisable()
    {
        _currentFocus?.OnFocusExit();
        _currentFocus = null;
    }
}