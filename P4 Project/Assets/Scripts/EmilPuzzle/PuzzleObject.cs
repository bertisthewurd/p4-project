using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))] // This attribute tells Unity that this script REQUIRES an AudioSource component on the same GameObject.
public class PuzzleObject : Interactable  // Inherit from Interactable so the team's PlayerInteract handles input for us
{

    public string objectName;  // "Shoes", "Typewriter", etc.
    public PuzzleSoundData correctSound;   // Refference to PuzzleSoundData asset that should be attached to the object when puzzle is solved 
    public PuzzleSoundData startingSound;  // Refference to PuzzleSoundData asset on the object at the start (the scrambled one)
    
    private AudioSource audioSource;  //Audiosource component
    private PuzzleSoundData currentSound;  // Sound currently assigned to the object

    public bool IsCorrect => currentSound == correctSound;  //Returns true if currentSound is the right one
    public bool IsEmpty => currentSound == null;  // Returns true when object has no sound 
    
    private bool playerInRange = false;
    private bool isLocked = false; // Once correct, can no longer be swapped
    private bool wasCorrectLastFrame = false; // Tracks transition from incorrect to correct

    public GameObject noteIconObject;
    public GameObject emptyIconObject;
    public Image noteIconImage;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        AssignSound(startingSound);  //Set puzzle sounds to the scrambled state
        UpdateIconVisibility(false);  //Hide icons since player is not in range
        promptMessage = "Press E to swap sound";  // Text shown by PlayerUI when looking at this object
    }

    // Called by PlayerInteract when player presses E while looking at this object
    protected override void Interact()
    {
        if (isLocked) return;  // Locked objects cannot be interacted with
        PuzzleManager.Instance.OnObjectClicked(this);
    }

    public void AssignSound(PuzzleSoundData newSound) //Used when objects sound changes (on awake and when player swaps)
    {
        if (isLocked) return;  // Locked objects cannot have their sound changed
        
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
        if (noteIconImage != null && newSound != null && newSound.icon !=null)  //Update world-space icon to match the current sound
        {
            noteIconImage.sprite = newSound.icon;
        }
        
        UpdateIconVisibility(playerInRange);
        
        Debug.Log($"{objectName} now has sound: {(newSound != null ? newSound.soundName : "EMPTY")}");
        // Check if this assignment made the object correct
        if (!wasCorrectLastFrame && IsCorrect)
        {
            OnCorrectPlacement();
        }
        wasCorrectLastFrame = IsCorrect;
    }
    
    // Called when this object transitions from incorrect to correct
    private void OnCorrectPlacement()
    {
        Debug.Log($"{objectName} correctly placed!");
        
        isLocked = true;
        promptMessage = "";  // No prompt for locked objects
        
        // Stop the ambient looping sound
        audioSource.Stop();
        audioSource.clip = null;
        
        // Hide both icons - this object is done
        if (noteIconObject != null) noteIconObject.SetActive(false);
        if (emptyIconObject != null) emptyIconObject.SetActive(false);
        
        // Play the win soundbite (currently a stub for FMOD wiring tomorrow)
        PlayWinSoundbite();
        
        // Notify the manager so it can track total solved count
        PuzzleManager.Instance.OnObjectSolved(this);
    }
    
    // Stub: tomorrow this will trigger the FMOD voiceline event for this object
    private void PlayWinSoundbite()
    {
        Debug.Log($"[STUB] Would play voiceline for {objectName}");
        // TODO: Replace with FMOD event call once voicelines are integrated
        // Example: FMODUnity.RuntimeManager.PlayOneShotAttached(winSoundbiteEvent, gameObject);
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

    private void UpdateIconVisibility(bool inRange)
    {
        if (isLocked)
        {
            // Locked objects show no icons
            if (noteIconObject != null) noteIconObject.SetActive(false);
            if (emptyIconObject != null) emptyIconObject.SetActive(false);
            return;
        }
    
        if (noteIconObject != null)
            noteIconObject.SetActive(inRange && !IsEmpty);
        if (emptyIconObject != null)
            emptyIconObject.SetActive(inRange && IsEmpty);
    }
}