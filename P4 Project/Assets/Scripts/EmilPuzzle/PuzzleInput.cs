using UnityEngine;

public class PuzzleInput : MonoBehaviour
{
   void Update()
   {
      if (Input.GetKeyDown(KeyCode.Q))
      {
         if (PuzzleManager.Instance != null)
            PuzzleManager.Instance.PlayCarriedPreview();
      }
   }
}
