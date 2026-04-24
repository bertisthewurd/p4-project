using UnityEngine;
using UnityEngine.Video;

public class SceneManager : MonoBehaviour
{
    // Jagged array storing scene IDs for each sequence.
    // First dimension = sequence index, second dimension = scene IDs within that sequence.
    public int[][] sceneMatrix = new int[6][];
    public string[][] sceneNames = new string[6][]; // Display names for each scene, matching sceneMatrix
    public int currentSequence = 0;
    public int currentScene = 0;
    public VideoPlayer videoPlayer;

    public void AssignSequenceStructure()
    {
        for (int i = 0; i < sceneMatrix.Length; i++)
        {
            switch (i)
            {
                case 1:
                    // Sequence 1 has 2 scenes (IDs 0 and 1)
                    sceneMatrix[i] = new int[] { 0, 1 };
                    sceneNames[i] = new string[] { "Turn Inward", "Retaliate" };
                    break;
                case 3:
                    // Sequence 3 has 4 scenes (IDs 0, 1, 2, 3)
                    sceneMatrix[i] = new int[] { 0, 1, 2, 3 };
                    sceneNames[i] = new string[] { "Tell her to fuck off", "Say the truth", "Lie", "Say nothing" };
                    break;
                case 5:
                    // Sequence 5 has 4 scenes (IDs 0, 1, 2, 3)
                    sceneMatrix[i] = new int[] { 0, 1, 2, 3 };
                    sceneNames[i] = new string[] { "Ask who they are", "Ask what they want", "Ask about your daughter", "Say nothing" };
                    break;
                default:
                    // All other sequences have 1 scene (ID 0)
                    sceneMatrix[i] = new int[] { 0 };
                    sceneNames[i] = new string[] { "Seq" + i + "_Scene0" };
                    break;
            }
        }
    }

    public float CalculateVideoLength()
    {
        ulong frameCount = videoPlayer.frameCount;
        float frameRate = videoPlayer.frameRate;
        float lengthInSeconds = frameCount / frameRate;
        return lengthInSeconds;
    }
}