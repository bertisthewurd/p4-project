using UnityEngine;
using FMODUnity;

public class PlayerFootsteps : MonoBehaviour
{
    [Header("FMOD Events")]
    [SerializeField] private EventReference walkFootstepEvent;
    [SerializeField] private EventReference sprintFootstepEvent;
    [SerializeField] private EventReference jumpEvent;
    [SerializeField] private EventReference landingEvent;

    [Header("Timing")]
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float sprintStepInterval = 0.3f;

    private CharacterController controller;
    private float stepTimer = 0f;
    private bool isSprinting = false;
    private bool wasGrounded = true;

    [Header("Keys")]
    public KeyCode sprintKey;
    public KeyCode jumpKey;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isSprinting = Input.GetKey(sprintKey);
        bool isGrounded = controller.isGrounded;

        // Jump sound — player just left the ground by pressing jump
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            RuntimeManager.PlayOneShot(jumpEvent, transform.position);
        }

        // Landing sound — player was in the air last frame and is now grounded
        if (!wasGrounded && isGrounded)
        {
            RuntimeManager.PlayOneShot(landingEvent, transform.position);
        }

        wasGrounded = isGrounded;

        // Footsteps
        bool isMoving = isGrounded && controller.velocity.magnitude > 0.1f;

        if (isMoving)
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = isSprinting ? sprintStepInterval : walkStepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    void PlayFootstep()
    {
        if (isSprinting)
        {
            RuntimeManager.PlayOneShot(sprintFootstepEvent, transform.position);
        }
        else
        {
            RuntimeManager.PlayOneShot(walkFootstepEvent, transform.position);
        }
    }
}