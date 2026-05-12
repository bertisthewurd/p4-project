using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    [Tooltip("Total number of puzzles in the game.")]
    [SerializeField] private int totalPuzzles = 2;

    private int puzzlesCompleted = 0;

    public float CompletionPercent => (puzzlesCompleted / (float)totalPuzzles) * 100f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void CompletePuzzle(string puzzleName)
    {
        puzzlesCompleted++;
        puzzlesCompleted = Mathf.Min(puzzlesCompleted, totalPuzzles);

        float percent = CompletionPercent;
        Debug.Log($"Puzzle '{puzzleName}' completed. Progress: {percent}%");
    }
}