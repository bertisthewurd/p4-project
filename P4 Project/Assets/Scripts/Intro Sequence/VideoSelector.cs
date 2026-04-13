using UnityEngine;
using UnityEngine.Video;

public class VideoSelector : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    private string videoBasePath;

    void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoBasePath = "file:///" + Application.dataPath.Replace(" ", "%20") + "/Videos/";
        SceneSelector.playScene += DisplayVideo;
        Debug.Log("VideoSelector started. Base path: " + videoBasePath);
    }

    void OnDestroy()
    {
        // Always unsubscribe to avoid memory leaks and phantom callbacks.
        SceneSelector.playScene -= DisplayVideo;
    }

    // sequenceIndex: which sequence we are in.
    // sceneID:       the chosen scene index within that sequence.
    private void DisplayVideo(int sequenceIndex, int sceneID)
    {
        string videoPath = null;

        switch (sequenceIndex)
        {
            case 0:
                // Sequence 0 has only one scene.
                videoPath = videoBasePath + "testVideo.mp4";
                break;

            case 1:
                switch (sceneID)
                {
                    case 0: videoPath = videoBasePath + "testVideo.mp4"; break;
                    case 1: videoPath = videoBasePath + "testVideo.mp4"; break;
                    default: Debug.LogWarning($"Sequence {sequenceIndex}: unrecognised scene ID {sceneID}"); break;
                }
                break;

            case 2:
                videoPath = videoBasePath + "Seq2_Scene0.mp4";
                break;

            case 3:
                switch (sceneID)
                {
                    case 0: videoPath = videoBasePath + "Seq3_Scene0.mp4"; break;
                    case 1: videoPath = videoBasePath + "Seq3_Scene1.mp4"; break;
                    case 2: videoPath = videoBasePath + "Seq3_Scene2.mp4"; break;
                    case 3: videoPath = videoBasePath + "Seq3_Scene3.mp4"; break;
                    default: Debug.LogWarning($"Sequence {sequenceIndex}: unrecognised scene ID {sceneID}"); break;
                }
                break;

            case 4:
                videoPath = videoBasePath + "Seq4_Scene0.mp4";
                break;

            case 5:
                switch (sceneID)
                {
                    case 0: videoPath = videoBasePath + "Seq5_Scene0.mp4"; break;
                    case 1: videoPath = videoBasePath + "Seq5_Scene1.mp4"; break;
                    case 2: videoPath = videoBasePath + "Seq5_Scene2.mp4"; break;
                    case 3: videoPath = videoBasePath + "Seq5_Scene3.mp4"; break;
                    default: Debug.LogWarning($"Sequence {sequenceIndex}: unrecognised scene ID {sceneID}"); break;
                }
                break;

            default:
                Debug.LogWarning($"Unrecognised sequence index: {sequenceIndex}");
                break;
        }

        if (videoPath != null)
        {
            Debug.Log("Attempting to play: " + videoPath);
            videoPlayer.url = videoPath;
            videoPlayer.Play();
        }
    }
}