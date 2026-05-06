using UnityEngine;

[RequireComponent(typeof(AudioSource))] // This attribute tells Unity that this script REQUIRES an AudioSource component on the same GameObject.
public class PuzzleObject : MonoBehaviour
{

    public string objectName;  // "Shoes", "Typewriter", etc.
    public PuzzleSoundData correctSound;   // Refference to PuzzleSoundData asset that should be attached to the object when puzzle is solved 
    public PuzzleSoundData startingSound;  // Refference to PuzzleSoundData asset on the object at the start (the scrambled one)
    
    private AudioSource audioSource;  //Audiosource component
    private PuzzleSoundData currentSound;  // Sound currently assigned to the object

    public bool IsCorrect => currentSound == correctSound;  //Returns true if currentSound is the right one
    public bool IsEmpty => currentSound == null;  // Returns true when object has no sound 
    
    private bool playerInRange = false; 

    public GameObject noteIconObject;
    public GameObject emptyIconObject;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        AssignSound(startingSound);  //Set puzzle sounds to the scrambled state
        UpdateIconVisibility(false);  //Hide icons since player is not in range
    }

    public void AssignSound(PuzzleSoundData newSound) //Used when objects sound changes (on awake and when player swaps)
    {
        currentSound = newSound;

        if (newSound != null && newSound.ambientLoop != null)  //Check if there is valid sound and audioclip
        {
            audioSource.clip = newSound.ambientLoop;  //Hand audioclip to audiosorce and loop it
            audioSource.loop = true;
            
            if (playerInRange)
                audioSource.Play();
        }
        else
        {
            audioSource.Stop();  // No sound, sound go .....
            audioSource.clip = null;
        }
        
        UpdateIconVisibility(playerInRange);
        
        Debug.Log($"{objectName} now has sound: {(newSound != null ? newSound.soundName : "EMPTY")}");
    }
    
    public PuzzleSoundData GetCurrentSound() => currentSound;  // Other scripts can read what sound 

    
    // Below requires: this object has a Collider with "Is Trigger" checked, and one of the two colliders has a Rigidbody.
    void OnTriggerEnter(Collider other) // Triggers when other collider enters objects trigger collider
    {
        if (other.CompareTag("Player")) // react if thing entering has "player" tag
        {
            playerInRange = true;
            UpdateIconVisibility(true);

            if (currentSound != null && audioSource.clip != null && !audioSource.isPlaying) // Resume audio if there's a sound assigned
            {
                audioSource.Play();
            }
            
            Debug.Log($"Player entered range of {objectName}");
        }
    }

    void OnTriggerExit(Collider other)  // Unity calls this automatically when another collider leaves this object's trigger collider.
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            UpdateIconVisibility(false);
            
            audioSource.Stop();
            
            Debug.Log($"Player left range of {objectName}");
        }
    }
    
    public bool IsPlayerInRange() => playerInRange; //Proximity state usefull for other scripts

    private void UpdateIconVisibility(bool inRange) //Decides which icon should be visibly if any
    {
        if (noteIconObject != null)
            noteIconObject.SetActive(inRange && !IsEmpty);
        if (emptyIconObject != null)
            emptyIconObject.SetActive(inRange && IsEmpty);
    }
}
