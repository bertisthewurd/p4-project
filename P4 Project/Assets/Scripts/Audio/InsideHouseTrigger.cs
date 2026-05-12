using UnityEngine;

public class InsideHouseTrigger : MonoBehaviour
{
    public static bool PlayerInHouse { get; private set; } = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInHouse = true;
            GameAudioManager.Instance.SetInsideHouseMusic();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerInHouse = false;
            GameAudioManager.Instance.SetNormalMusic(GameProgressManager.Instance.CompletionPercent);
        }
    }
}