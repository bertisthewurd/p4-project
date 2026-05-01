using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance;
    private PuzzleSoundData carriedSound = null;

    void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }
    
    public void OnObjectClicked(PuzzleObject obj)
    {
        PuzzleSoundData objectSounds = obj.GetCurrentSound();
        obj.AssignSound(carriedSound);
        carriedSound = objectSounds;
        Debug.Log($"Now carrying: {(carriedSound != null ? carriedSound.soundName : "NOTHING")}");
    }

    public PuzzleSoundData GetCarriedSound() => carriedSound;
}
