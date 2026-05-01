using System;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
   public Camera playerCamera;
   public float maxClickDistance;

   private void Update()
   {
      if (Input.GetMouseButtonDown(0))
      {
         Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
         if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance))
         {
            PuzzleObject obj = hit.collider.GetComponent<PuzzleObject>();
            if (obj != null)
            {
               PuzzleManager.Instance.OnObjectClicked(obj);
            }
         }
      }
   }
}
