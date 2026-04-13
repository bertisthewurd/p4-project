using UnityEngine;
using UnityEngine.Video;

public class SceneManager : MonoBehaviour
{
    // Jagged array storing scene IDs for each sequence.
    // First dimension = sequence index, second dimension = scene IDs within that sequence.
    public int[][] sceneMatrix = new int[6][];
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
                    break;
                case 3:
                    // Sequence 3 has 4 scenes (IDs 0, 1, 2, 3)
                    sceneMatrix[i] = new int[] { 0, 1, 2, 3 };
                    break;
                case 5:
                    // Sequence 5 has 4 scenes (IDs 0, 1, 2, 3)
                    sceneMatrix[i] = new int[] { 0, 1, 2, 3 };
                    break;
                default:
                    // All other sequences have 1 scene (ID 0)
                    sceneMatrix[i] = new int[] { 0 };
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