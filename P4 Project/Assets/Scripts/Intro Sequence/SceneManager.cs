using UnityEngine;
using UnityEngine.Video;
public class SceneManager : MonoBehaviour
{
    //Creates a jagged array for storing scenes in a scene matrix. 
    //The first dimension is the number of overall sequences, and the second dimension is the individual scenes within each sequence.
    public int[][] sceneMatrix = new int[6][]; 
    public int currentSequence = 0; //Tracks the current sequence being played.
    public int currentScene = 0; //Tracks the current scene within the current sequence.
    public VideoPlayer videoPlayer; //Reference to the video player GameObject in the scene.
    public void AssignSequenceStructure()
    {
       for (int i = 0; i < sceneMatrix.Length; i++)
       {
        switch (i)
            {
                case 1:
                    sceneMatrix[i] = new int[2]; //Sequence 1 has 2 scenes
                    break;
                case 3:
                    sceneMatrix[i] = new int[4]; //Sequence 3 has 4 scenes
                    break;
                case 5:
                    sceneMatrix[i] = new int[4]; //Sequence 5 has 4 scenes
                    break;
                default:
                    sceneMatrix[i] = new int[1]; //All other sequences have 1 scene
                    break;
            }
        } 
    }

    public float CalculateVideoLength()
    {
        // Get frame count and frame rate from the VideoPlayer. 
        ulong frameCount = videoPlayer.frameCount;
        float frameRate = videoPlayer.frameRate;

        // Calculate the length in seconds. 
        float lengthInSeconds = frameCount / frameRate;

        return lengthInSeconds;
    }
}
