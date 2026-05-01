using UnityEngine;

[CreateAssetMenu(fileName = "PuzzleSoundData", menuName = "Puzzle/PuzzleSoundData")]
public class PuzzleSoundData : ScriptableObject
{
    public string soundName;
    public AudioClip ambientLoop;
}
