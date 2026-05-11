using UnityEngine;
using FMODUnity;

[CreateAssetMenu(fileName = "PuzzleSoundData", menuName = "Puzzle/PuzzleSoundData")]
public class PuzzleSoundData : ScriptableObject
{
    public string soundName; // Display name of sound 
    public AudioClip ambientLoop; //Sound file where you drag .wav into
    public Sprite icon;
    
    [Header("FMOD")]
    public EventReference ambientLoopEvent;   // The looping ambient sound 
    public EventReference winSoundbiteEvent;  // The voiceline played on correct placement 
}
