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
    void Start()
    {
        //cam = GetComponent<Camera>();
        playerUI = GetComponent<PlayerUI>();
    }

    void Update()
    {
        if (PickUp.IsHolding)
        {
            if (Input.GetKeyDown(KeyCode.E))
                PickUp.DropCurrent();
            playerUI.UpdateText(string.Empty);
            return;
        }

        playerUI.UpdateText(string.Empty);

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * distance);
        RaycastHit hitInfo;
        if (Physics.SphereCast(ray, 0.05f, out hitInfo, distance))
        {
            if(hitInfo.collider.GetComponent<Interactable>() != null)
            {
                Interactable interactable = hitInfo.collider.GetComponent<Interactable>();
                playerUI.UpdateText(interactable.promptMessage);
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.BaseInteract();
                }
            }
        }
    }
}
