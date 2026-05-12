using UnityEngine;

public class InsideHouseTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameAudioManager.Instance.SetInsideHouseMusic();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameAudioManager.Instance.SetNormalMusic(GameProgressManager.Instance.CompletionPercent);
        }
    }
}