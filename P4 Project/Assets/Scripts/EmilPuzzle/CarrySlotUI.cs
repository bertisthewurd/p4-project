using UnityEngine;
using UnityEngine.UI;

public class CarrySlotUI : MonoBehaviour // Shows the note icon when player is carrying a sound, hides it when not.
{
    public Image slotImage;
    public GameObject carryHint;
    
    public void UpdateDisplay(PuzzleSoundData carried) // Called by PuzzleManager whenever the carried sound changes.
    {
        if (carried != null && carried.icon != null)
        {
            slotImage.sprite = carried.icon;
            slotImage.gameObject.SetActive(true);
            if (carryHint != null) carryHint.SetActive(true);  // Show hint when carrying
        }
        else
        {
            slotImage.gameObject.SetActive(false);
            if (carryHint != null) carryHint.SetActive(false);  // Hide hint when not carrying
        }
    }
}
