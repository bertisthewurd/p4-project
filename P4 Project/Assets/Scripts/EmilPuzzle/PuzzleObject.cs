using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PuzzleObject : MonoBehaviour
{

    public string objectName;  // "Shoes", "Typewriter", etc.
    public PuzzleSoundData correctSound;   // What SHOULD be here 
    public PuzzleSoundData startingSound;  // What's here at puzzle start (the scrambled one)
    
    private AudioSource audioSource;
    private PuzzleSoundData currentSound;

    public bool IsCorrect => correctSound == correctSound;
    public bool IsEmpty => currentSound == null;
    
    private bool playerInRange = false;

    public GameObject noteIconObject;
    public GameObject emptyIconObject;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        AssignSound(startingSound);
        UpdateIconVisibility(false);
    }

    public void AssignSound(PuzzleSoundData newSound)
    {
        currentSound = newSound;

        if (newSound != null && newSound.ambientLoop != null)
        {
            audioSource.clip = newSound.ambientLoop;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
        
        Debug.Log($"{objectName} now has sound: {(newSound != null ? newSound.soundName : "EMPTY")}");
    }
    
    public PuzzleSoundData GetCurrentSound() => currentSound;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            UpdateIconVisibility(true);
            Debug.Log($"Player entered range of {objectName}");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            UpdateIconVisibility(false);
            Debug.Log($"Player left range of {objectName}");
        }
    }
    
    public bool IsPlayerInRange() => playerInRange;

    private void UpdateIconVisibility(bool inRange)
    {
        if (noteIconObject != null)
            noteIconObject.SetActive(inRange && !IsEmpty);
        if (emptyIconObject != null)
            emptyIconObject.SetActive(inRange && IsEmpty);
    }
}
