using UnityEngine;

public class MinistryOfficeTrigger : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameAudioManager.Instance.SetMinistryOfficeMusic();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameAudioManager.Instance.ExitMinistryOffice(GameProgressManager.Instance.CompletionPercent);
        }
    }
}

