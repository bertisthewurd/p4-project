using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

public class SceneSelector : SceneManager
{
    public static event Action<int, int> playScene; // Args: sequenceIndex, sceneID

    [Header("Scene Selection UI")]
    public GameObject narrativeOptionPrefab;  // Assign the "Narrative Option" prefab here
    public Transform uiCanvas;               // Assign the UI Canvas transform here
    public float buttonWidth = 400f;         // Width of each button
    public float buttonHeight = 45f;         // Height of each button
    public float buttonSpacing = 8f;         // Vertical gap between buttons
    public float bottomPadding = 60f;        // Distance from bottom of screen to lowest button

    private List<GameObject> activeNarrativeOptions = new List<GameObject>();

    [Header("Selection Timing")]
    [Tooltip("How many seconds before the current video ends that the selection buttons appear.")]
    public float[] visibleButtonTime = new float[6];
    private float buttonsAppearBeforeEnd = 5f;

    private int selectedScene = 0;
    private bool sceneWasSelected = false;
    private bool isChoosingScene = false;

    void Awake()
    {
        AssignSequenceStructure();
        videoPlayer = GetComponent<VideoPlayer>();
    }

    void Start()
    {
        PlaySequences();
    }

    public void PlaySequences()
    {
        Debug.Log("Starting scene sequence playback.");
        StartCoroutine(PlayAllSequences());
    }

    private IEnumerator PlayAllSequences()
    {
        while (currentSequence < sceneMatrix.Length)
        {
            if (sceneMatrix[currentSequence].Length == 1)
            {
                yield return StartCoroutine(PlaySceneCoroutine(currentSequence, 0));
            }
            else
            {
                if (!sceneWasSelected)
                {
                    yield return StartCoroutine(SelectScene());
                }

                yield return StartCoroutine(PlaySceneCoroutine(currentSequence, selectedScene));
                selectedScene = 0;
                sceneWasSelected = false;
            }

            currentSequence++;
            currentScene = 0;
        }
        
        Debug.Log("No More Sequences");
        Debug.Log("Attempting to load Main Scene");
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Scene");
    }

    private IEnumerator PlaySceneCoroutine(int sequenceIndex, int sceneIndex)
    {
        int sceneID = sceneMatrix[sequenceIndex][sceneIndex];
        Debug.Log($"Playing Sequence {sequenceIndex}, Scene {sceneID}");
        playScene?.Invoke(sequenceIndex, sceneID);
        buttonsAppearBeforeEnd = visibleButtonTime[sequenceIndex];

        // Wait until the VideoPlayer has fully prepared before reading its length.
        bool isPrepared = false;
        videoPlayer.prepareCompleted += _ => isPrepared = true;
        videoPlayer.Prepare();
        yield return new WaitUntil(() => isPrepared);

        float videoLength = CalculateVideoLength();
        float timeUntilButtons = videoLength - buttonsAppearBeforeEnd;

        // If the next sequence needs a choice, show buttons before this video ends.
        bool nextSequenceNeedsChoice = (currentSequence + 1 < sceneMatrix.Length) &&
                                       (sceneMatrix[currentSequence + 1].Length > 1);

        if (nextSequenceNeedsChoice && timeUntilButtons > 0f)
        {
            yield return new WaitForSeconds(timeUntilButtons);
            ShowSelectionButtons(currentSequence + 1);
            yield return new WaitForSeconds(buttonsAppearBeforeEnd);

            if (!sceneWasSelected)
            {
                selectedScene = 0;
                sceneWasSelected = true;
                isChoosingScene = false;
                DismissSelectionButtons();
            }
        }
        else
        {
            yield return new WaitForSeconds(videoLength);
        }
    }

    private IEnumerator SelectScene()
    {
        if (!isChoosingScene && !sceneWasSelected)
        {
            ShowSelectionButtons(currentSequence);
        }

        while (!sceneWasSelected && isChoosingScene)
        {
            yield return null;
        }

        if (!sceneWasSelected)
        {
            selectedScene = 0;
            sceneWasSelected = true;
            isChoosingScene = false;
            DismissSelectionButtons();
        }
    }

    // Spawns a vertical stack of buttons at the bottom centre of the screen, Telltale-style.
    // Index 0 is at the bottom, the last option is at the top.
    private void ShowSelectionButtons(int sequenceIndex)
    {
        isChoosingScene = true;

        int sceneCount = sceneMatrix[sequenceIndex].Length;

        for (int i = 0; i < sceneCount; i++)
        {
            int capturedIndex = i;

            GameObject buttonObj = Instantiate(narrativeOptionPrefab, uiCanvas);
            activeNarrativeOptions.Add(buttonObj);

            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot     = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(buttonWidth, buttonHeight);

            // Stack upward from the bottom: button 0 lowest, each one above the last.
            float yPos = bottomPadding + i * (buttonHeight + buttonSpacing);
            rect.anchoredPosition = new Vector2(0f, yPos);

            TMP_Text label = buttonObj.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = sceneNames[sequenceIndex][capturedIndex];
            }

            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnSceneSelected(capturedIndex, sequenceIndex));
            }
        }
    }

    private void OnSceneSelected(int sceneIndex, int sequenceIndex)
    {
        selectedScene = sceneIndex;
        sceneWasSelected = true;
        isChoosingScene = false;
        DismissSelectionButtons();
    }

    private void DismissSelectionButtons()
    {
        foreach (GameObject btn in activeNarrativeOptions)
        {
            if (btn != null)
                Destroy(btn);
        }
        activeNarrativeOptions.Clear();
    }
}