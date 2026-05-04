using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleSoundData", menuName = "Puzzle/PuzzleSoundData")]
public class PuzzleSoundData : ScriptableObject
{
    public string soundName; // Display name of sound 
    public AudioClip ambientLoop; //Sound file where you drag .wav into
}
