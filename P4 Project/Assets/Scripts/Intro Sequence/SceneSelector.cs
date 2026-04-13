using UnityEngine;
using UnityEngine.Video;
using System.Collections;
using System;

public class SceneSelector : SceneManager
{
    public static event Action<int> playSequence; //Action to trigger sequence playback.
    void Awake()
    {
        AssignSequenceStructure(); //Initialize the scene matrix structure when the object awakens.
        videoPlayer = GetComponent<VideoPlayer>(); //Get the existing VideoPlayer component from this GameObject.
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //videoPlayer.url = "Assets/Videos/testVideo.mp4"; //Set the initial video clip to play.
        //videoPlayer.Play(); //Start playing the video.
    }
    public IEnumerator PlaySequences()
    {
        for (currentSequence = 0; currentSequence < sceneMatrix.Length; currentSequence++)
        {
            if (sceneMatrix[currentSequence].Length == 1)
            {
                playSequence?.Invoke(sceneMatrix[currentSequence][0]); //Invoke the playSequence action, passing the current sequence as an argument.                yield return new WaitForSeconds(CalculateVideoLength());
            }
            else
            {
               SelectScene();
            }
        }
        playSequence?.Invoke(sceneMatrix[currentSequence][currentScene]); //Invoke the playSequence action, passing the current sequence as an argument.
        yield return new WaitForSeconds(CalculateVideoLength());
    }
    
    void SelectScene()
    {
        //This method can be expanded to include logic for selecting specific scenes based on user input or other conditions.
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
