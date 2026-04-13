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
    public float buttonSpacing = 220f;       // Preferred spacing between buttons
    public float buttonMinWidth = 180f;      // Minimum width for each button
    public float buttonMaxWidth = 360f;      // Maximum width for each button
    public float horizontalMargin = 24f;     // Margin from screen edges
    public float bottomPadding = 24f;        // Space from bottom of screen

    private List<GameObject> activeNarrativeOptions = new List<GameObject>();

    [Header("Selection Timing")]
    [Tooltip("How many seconds before the current video ends that the selection buttons appear.")]
    public float buttonsAppearBeforeEnd = 5f; // Designers can tweak this in the Inspector

    private int selectedScene = 0;           // Defaults to scene index 0 in the sequence
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

    // Drives the full sequence playback from start to finish.
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
                // Single-scene sequence: play it directly, no choice needed.
                yield return StartCoroutine(PlaySceneCoroutine(currentSequence, 0));
            }
            else
            {
                // Multi-scene sequence: if a choice has already been made while the previous video was playing,
                // use it immediately. Otherwise, wait for the player to choose.
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
    }

    // Plays a specific scene by invoking the playScene event.
    private IEnumerator PlaySceneCoroutine(int sequenceIndex, int sceneIndex)
    {
        int sceneID = sceneMatrix[sequenceIndex][sceneIndex];
        Debug.Log($"Playing Sequence {sequenceIndex}, Scene {sceneID}");
        playScene?.Invoke(sequenceIndex, sceneID);

        // Prepare the VideoPlayer and wait until it is ready before reading its length.
        bool isPrepared = false;
        videoPlayer.prepareCompleted += _ => isPrepared = true;
        videoPlayer.Prepare();
        yield return new WaitUntil(() => isPrepared);

        float videoLength = CalculateVideoLength();
        float timeUntilButtons = videoLength - buttonsAppearBeforeEnd;

        // If the NEXT sequence requires a choice, show buttons before this video ends.
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

    // Waits for the player to pick a scene (or defaults to scene 0 if none chosen).
    private IEnumerator SelectScene()
    {
        // Buttons may already be showing (spawned during the previous video).
        // If not (e.g. very first sequence), show them now.
        if (!isChoosingScene && !sceneWasSelected)
        {
            ShowSelectionButtons(currentSequence);
        }

        // Wait until the player selects, or the selection defaults after the previous video ended.
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

    // Spawns a horizontal row of buttons for each scene in the given sequence.
    private void ShowSelectionButtons(int sequenceIndex)
    {
        isChoosingScene = true;

        int sceneCount = sceneMatrix[sequenceIndex].Length;
        RectTransform canvasRect = uiCanvas.GetComponent<RectTransform>();
        float canvasWidth = canvasRect.rect.width;
        float canvasHeight = canvasRect.rect.height;

        // Choose a button width that fits evenly across the available width.
        float availableWidth = Mathf.Max(0f, canvasWidth - 2f * horizontalMargin);
        float targetButtonWidth = Mathf.Clamp((availableWidth - (sceneCount - 1) * buttonSpacing) / sceneCount, buttonMinWidth, buttonMaxWidth);
        float actualSpacing = Mathf.Max(10f, (availableWidth - targetButtonWidth * sceneCount) / Mathf.Max(1, sceneCount - 1));
        float totalWidth = sceneCount * targetButtonWidth + (sceneCount - 1) * actualSpacing;
        float startX = -totalWidth / 2f + targetButtonWidth / 2f;
        float buttonY = bottomPadding + (canvasHeight * 0.05f);
        float buttonHeight = Mathf.Clamp(canvasHeight * 0.12f, 70f, 110f);

        for (int i = 0; i < sceneCount; i++)
        {
            int capturedIndex = i; // Capture for the lambda closure.

            GameObject buttonObj = Instantiate(narrativeOptionPrefab, uiCanvas);
            activeNarrativeOptions.Add(buttonObj);
            RectTransform rect = buttonObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.sizeDelta = new Vector2(targetButtonWidth, buttonHeight);
            rect.anchoredPosition = new Vector2(startX + i * (targetButtonWidth + actualSpacing), buttonY);

            // Set button label using TMP_Text.
            TMP_Text label = buttonObj.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = "Scene " + (capturedIndex + 1);
            }

            // Wire up the click callback.
            Button btn = buttonObj.GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.AddListener(() => OnSceneSelected(capturedIndex, sequenceIndex));
            }
        }
    }

    // Called when the player clicks a scene button.
    private void OnSceneSelected(int sceneIndex, int sequenceIndex)
    {
        selectedScene = sceneIndex;
        sceneWasSelected = true;
        isChoosingScene = false;
        DismissSelectionButtons();
    }

    // Destroys all active button instances from the canvas.
    private void DismissSelectionButtons()
    {
        foreach (GameObject btn in activeNarrativeOptions)
        {
            if (btn != null)
            {
                Destroy(btn);
            }
        }

        activeNarrativeOptions.Clear();
    }
}