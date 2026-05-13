using UnityEngine;
public class SceneResetTool : MonoBehaviour
{
    [SerializeField] private string mainSceneName = "Main";
    [SerializeField] private Vector3 spawnPosition;

    void Update()
    {
        bool modifier = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand);

        if (modifier && Input.GetKeyDown(KeyCode.R))
        {
            // Reset player position
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                player.transform.position = spawnPosition;
                if (cc != null) cc.enabled = true;
            }

            UnityEngine.SceneManagement.SceneManager.LoadScene(mainSceneName);
        }
    }
}