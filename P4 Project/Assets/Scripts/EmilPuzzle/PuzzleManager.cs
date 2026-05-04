using UnityEngine;

public class PuzzleManager : MonoBehaviour //Handles swap logic and when object is clicked
{
    public static PuzzleManager Instance; // Singleton reference
    private PuzzleSoundData carriedSound = null; // The sound the player is currently holding.

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);  // If another PuzzleManager already exists, destroy the duplicate.
        else Instance = this;
    }
    
    // Called by PlayerInteraction script when the player clicks a puzzle object.
    // Performs the swap: object's current sound goes to player, player's carried sound goes to object
    public void OnObjectClicked(PuzzleObject obj)  
    {
        PuzzleSoundData objectSounds = obj.GetCurrentSound(); // Remember what sound the object currently has
        obj.AssignSound(carriedSound); // Give the object whatever we were carrying
        carriedSound = objectSounds;  // Carry sound that was on object
        Debug.Log($"Now carrying: {(carriedSound != null ? carriedSound.soundName : "NOTHING")}");
    }

    public PuzzleSoundData GetCarriedSound() => carriedSound;
}
