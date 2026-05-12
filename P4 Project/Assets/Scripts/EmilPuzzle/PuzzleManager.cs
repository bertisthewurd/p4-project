using UnityEngine;
using FMOD.Studio;
using FMODUnity;

public class PuzzleManager : MonoBehaviour //Handles swap logic and when object is clicked
{
    public static PuzzleManager Instance; // Singleton reference
    private PuzzleSoundData carriedSound = null; // The sound the player is currently holding.
    private WinCon winConManager;
    
    public CarrySlotUI carrySlotUI;
    
    private int solvedCount = 0;
    public int totalObjects = 7;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);  // If another PuzzleManager already exists, destroy the duplicate.
        else Instance = this;

        winConManager = GameObject.Find("Win Con Manager").GetComponent<WinCon>();
    }

    // Play the currently carried sound
    public void PlayCarriedPreview()
    {
        if (carriedSound == null || carriedSound.ambientLoopEvent.IsNull) return;

        EventInstance previewInstance = RuntimeManager.CreateInstance(carriedSound.ambientLoopEvent);
        previewInstance.start();
        
        // Attach to the player so FMOD has a 3D position
        RuntimeManager.AttachInstanceToGameObject(previewInstance, Camera.main.transform);

        // stop after designated amount of time
        StartCoroutine(StopPreviewAfterDelay(previewInstance, 5f));
    }

    private System.Collections.IEnumerator StopPreviewAfterDelay(EventInstance preview, float delay)
    {
        yield return new WaitForSeconds(delay);
        preview.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        preview.release();
    }

    // Called by PlayerInteraction script when the player clicks a puzzle object.
    // Performs the swap: object's current sound goes to player, player's carried sound goes to object
    public void OnObjectClicked(PuzzleObject obj)  
    {
        PuzzleSoundData objectSounds = obj.GetCurrentSound(); // Remember what sound the object currently has
        obj.AssignSound(carriedSound); // Give the object whatever we were carrying
        carriedSound = objectSounds;  // Carry sound that was on object
        
        if (carrySlotUI != null) // Update the on-screen slot to reflect what we're now carrying.
            carrySlotUI.UpdateDisplay(carriedSound);
        
        Debug.Log($"Now carrying: {(carriedSound != null ? carriedSound.soundName : "NOTHING")}");
    }

    // Called by PuzzleObject when it transitions to correct
    public void OnObjectSolved(PuzzleObject obj)
    {
        solvedCount++;
        Debug.Log($"Solved {solvedCount}/{totalObjects}: {obj.name}");
        
        if (solvedCount >= totalObjects)
        {
            OnPuzzleComplete();
        }
    }
    
    // Called when all 7 objects are correctly placed
    private void OnPuzzleComplete()
    {
        Debug.Log("PUZZLE COMPLETE!");
        // Tomorrow: trigger final memory reveal sequence here
        winConManager.PlayEmilWin();
    }

    public PuzzleSoundData GetCarriedSound() => carriedSound;
}
