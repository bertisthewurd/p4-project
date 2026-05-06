using System;
using UnityEngine;


// Handles the player's click input. Attached to the Player GameObject.
// Detects clicks on puzzle objects and notifies the PuzzleManager.
public class PlayerInteraction : MonoBehaviour
{
   public Camera playerCamera; // The camera used to shoot rays from.
   public float maxClickDistance;

   private void Update()
   {
      if (Input.GetMouseButtonDown(0))
      {
         Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition); // Makes ray that starts at camera and goes through mouse cursor position
         if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance)) //Shoot ray. If hits collider within range, stores info about the hit
         {
            PuzzleObject obj = hit.collider.GetComponentInParent<PuzzleObject>(); // Check if the thing we hit has a PuzzleObject component on it.
            if (obj != null) // If yes, tell the PuzzleManager to handle the click.
            {
               PuzzleManager.Instance.OnObjectClicked(obj);
            }
         }
      }
   }
}
