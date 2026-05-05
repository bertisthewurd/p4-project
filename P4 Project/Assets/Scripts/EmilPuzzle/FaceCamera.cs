using UnityEngine;

public class FaceCamera : MonoBehaviour //Script makes object its attached to always face the camera
{
   void LateUpdate() // Since Camera runs on Update, we use LateUpdate. It guarantees the camera has finished moving before we react to it.
   {
      if (Camera.main != null)
         transform.rotation =  Camera.main.transform.rotation; //copies cameras rotation into gameobjects rotation
   }
}
