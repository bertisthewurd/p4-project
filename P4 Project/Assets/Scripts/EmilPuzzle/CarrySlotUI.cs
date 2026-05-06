using UnityEngine;
using UnityEngine.UI;

public class CarrySlotUI : MonoBehaviour // Shows the note icon when player is carrying a sound, hides it when not.
{
    public Image slotImage;
    
    public void UpdateDisplay(PuzzleSoundData carried) // Called by PuzzleManager whenever the carried sound changes.
    {
        slotImage.gameObject.SetActive(carried != null); // Show the slot if carrying something, hide it if not.
    }
}
