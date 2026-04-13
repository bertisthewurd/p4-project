using System;
using UnityEngine;

public class VideoSelector : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SceneSelector.playSequence += DisplayVideo; //Subscribe the DisplayVideo method to the playSequence event.
    }

    private void DisplayVideo(int sceneID)
    {
        switch (sceneID)
        {
            case 0:
                Debug.Log(sceneID);
                break;
            case 1:
                //Logic to display video for scene 1
                break;
            case 2:
                //Logic to display video for scene 2
                break;
            //Add more cases as needed for additional scenes.
            default:
                Debug.LogWarning("Scene ID not recognized: " + sceneID);
                break;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
