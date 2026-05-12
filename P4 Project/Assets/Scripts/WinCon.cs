using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WinCon : MonoBehaviour
{
    public AudioClip emilWin, mikkelWin, gameWin;
    public float emilSoundLength, mikkelSoundLength;
    public float screenFadeTime, quitTime;
    private AudioSource audioSource;
    private bool onePuzzleDone = false;
    private bool fadeScreen = false;
    
    private GameObject player;
    public RawImage fadeImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //DEBUG stuff lmao
        if (Input.GetKeyDown(KeyCode.I))
        {
            PlayEmilWin();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {  
            PlayMikkelWin();
        }

        if (fadeScreen)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, fadeImage.color.a + Time.deltaTime / screenFadeTime);
        }
    }

    public void PlayEmilWin()
    {
        Debug.Log("Win Con says: Emil Done");
        audioSource.PlayOneShot(emilWin);
        if (onePuzzleDone)
        {
            StartCoroutine(PlayGameWin(emilSoundLength));
        }
        else
        {
            onePuzzleDone = true;
        }
    }

    public void PlayMikkelWin()
    {
        Debug.Log("Win Con says: Mikkel Done");
        audioSource.PlayOneShot(mikkelWin);
        if (onePuzzleDone)
        {
            StartCoroutine(PlayGameWin(mikkelSoundLength));
        }
        else
        {
            onePuzzleDone = true;
        }
    }

    private IEnumerator PlayGameWin(float soundLength)
    {
        yield return new WaitForSeconds(soundLength);
        audioSource.PlayOneShot(gameWin);
        fadeScreen = true;
        yield return new WaitForSeconds(quitTime);
        Application.Quit();
    }
}
