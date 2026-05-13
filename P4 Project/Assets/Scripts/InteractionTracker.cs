using UnityEngine;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Static interaction tracker. Call from any script:
///   InteractionTracker.Log("pickup");
///   InteractionTracker.Log("putdown");
///   InteractionTracker.Log("place");
///   InteractionTracker.Log("swap");
///
/// At the end of the session, call:
///   InteractionTracker.Export();
///
/// Or it auto-exports when the application quits.
/// 
/// Setup: Attach this to a single GameObject in your starting scene.
/// Set participantId in the Inspector before each session.
/// </summary>
public class InteractionTracker : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Auto-generated session ID. Assign participant ID when merging CSVs.")]
    public string sessionId = "";

    [Tooltip("Leave empty to save next to the game executable. Or set a custom folder path.")]
    public string customSavePath = "";

    // Singleton
    private static InteractionTracker _instance;

    // Event storage
    private static List<LogEntry> _log = new List<LogEntry>();
    private static Dictionary<string, int> _counts = new Dictionary<string, int>();
    private static float _sessionStartTime;
    private static bool _exported = false;

    private struct LogEntry
    {
        public float gameTime;
        public string entry;
        public string type;
    }

    private void Awake()
    {
        // Singleton pattern - survives scene loads
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        // Auto-generate session ID from timestamp
        if (string.IsNullOrEmpty(sessionId))
            sessionId = "session_" + System.DateTime.Now.ToString("HH-mm-ss");

        // Initialize
        _sessionStartTime = Time.realtimeSinceStartup;
        _log.Clear();
        _counts.Clear();
        _exported = false;

        Debug.Log($"[InteractionTracker] Ready. Session: {sessionId}");
        string saveTo = string.IsNullOrEmpty(customSavePath)
            ? (Application.isEditor ? Application.dataPath : Path.GetDirectoryName(Application.dataPath))
            : customSavePath;
        Debug.Log($"[InteractionTracker] CSV will save to: {saveTo}");
    }

    /// <summary>
    /// Log an interaction event. Call from anywhere:
    ///   InteractionTracker.Log("pickup");
    /// </summary>
    public static void Log(string interactionType)
    {
        if (_instance == null)
        {
            Debug.LogWarning("[InteractionTracker] No instance found. Add InteractionTracker to a GameObject.");
            return;
        }

        string type = interactionType.ToLower().Trim();

        // Update count
        if (!_counts.ContainsKey(type))
            _counts[type] = 0;
        _counts[type]++;

        // Create log entry
        float elapsed = Time.realtimeSinceStartup - _sessionStartTime;
        LogEntry entry = new LogEntry
        {
            gameTime = elapsed,
            entry = $"{type} #{_counts[type]}",
            type = type
        };
        _log.Add(entry);

        Debug.Log($"[InteractionTracker] {FormatTime(elapsed)} | {entry.entry}");
    }

    /// <summary>
    /// Export the log to CSV. Called automatically on quit,
    /// or call manually: InteractionTracker.Export();
    /// </summary>
    public static void Export()
    {
        if (_instance == null || _exported) return;
        _exported = true;

        string pid = _instance.sessionId;
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"interactions_{pid}_{timestamp}.csv";

        // Determine save folder
        string folder;
        if (!string.IsNullOrEmpty(_instance.customSavePath))
        {
            folder = _instance.customSavePath;
        }
        else
        {
            // Save next to the executable (or in project root in editor)
            folder = Application.dataPath;
            // In a build, Application.dataPath points inside the app bundle/data folder.
            // Go up one level to land next to the executable.
            if (!Application.isEditor)
                folder = Path.GetDirectoryName(folder);
        }

        // Ensure the folder exists
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string path = Path.Combine(folder, filename);

        using (StreamWriter writer = new StreamWriter(path))
        {
            // Summary section
            writer.WriteLine("Participant,Metric,Value");

            float totalTime = Time.realtimeSinceStartup - _sessionStartTime;
            writer.WriteLine($"{pid},Total_Session_Time,{FormatTime(totalTime)}");
            writer.WriteLine($"{pid},Total_Session_Seconds,{Mathf.RoundToInt(totalTime)}");

            int totalInteractions = 0;
            foreach (var kvp in _counts)
            {
                string metricName = kvp.Key.Substring(0, 1).ToUpper() + kvp.Key.Substring(1) + "_Count";
                writer.WriteLine($"{pid},{metricName},{kvp.Value}");
                totalInteractions += kvp.Value;
            }
            writer.WriteLine($"{pid},Total_Interactions,{totalInteractions}");

            // Log section
            writer.WriteLine();
            writer.WriteLine("Participant,Game_Time,Entry,Type");

            foreach (LogEntry entry in _log)
            {
                string safeEntry = entry.entry.Replace("\"", "\"\"");
                writer.WriteLine($"{pid},{FormatTime(entry.gameTime)},\"{safeEntry}\",{entry.type}");
            }
        }

        Debug.Log($"[InteractionTracker] CSV exported to: {path}");

        // Show path on screen briefly in builds
        if (!Application.isEditor)
        {
            Debug.Log($"File saved: {path}");
        }
    }

    private static string FormatTime(float seconds)
    {
        int total = Mathf.RoundToInt(seconds);
        int h = total / 3600;
        int m = (total % 3600) / 60;
        int s = total % 60;
        return $"{h:D2}:{m:D2}:{s:D2}";
    }

    // Auto-export when the game closes
    private void OnApplicationQuit()
    {
        Export();
    }
}