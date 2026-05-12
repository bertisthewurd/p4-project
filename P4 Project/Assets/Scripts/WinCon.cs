using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WinCon : MonoBehaviour
{
    public static WinCon Instance { get; private set; }

    public float screenFadeTime, quitTime;
    private bool onePuzzleDone = false;
    private bool fadeScreen = false;

    private GameObject player;
    public RawImage fadeImage;

    void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        //audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

        if (fadeScreen)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, fadeImage.color.a + Time.deltaTime / screenFadeTime);
        }
    }

    public void PlayEmilWin()
    {
        Debug.Log("Win Con says: Emil Done");
        if (onePuzzleDone)
        {
            StartCoroutine(PlayGameWin());
        }
        else
        {
            onePuzzleDone = true;
        }
    }

    public void PlayMikkelWin()
    {
        Debug.Log("Win Con says: Mikkel Done");
        if (onePuzzleDone)
        {
            StartCoroutine(PlayGameWin());
        }
        else
        {
            onePuzzleDone = true;
        }
    }

    private IEnumerator PlayGameWin()
    {
        fadeScreen = true;
        yield return new WaitForSeconds(screenFadeTime + quitTime);
        Application.Quit();
    }
}
