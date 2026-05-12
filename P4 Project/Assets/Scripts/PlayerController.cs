using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private CharacterController controller;
    private GameObject head;

    [Header("Camera")]
    public float Sensitivity;
    public float verticalClamp;
    public Vector2 rotation = Vector2.zero;

    [Header("Movement")]
    public float walkingSpeed = 5f;
    public float sprintingSpeed = 8f;

    [Header("Jumping")]
    public float jumpForce = 5f;
    public float gravity = -20f;

    [Header("Keys")]
    public KeyCode forward;
    public KeyCode backward, left, right, sprint, jump;

    private float topSpeed;
    private float verticalVelocity = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        head = transform.GetChild(0).gameObject;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        topSpeed = walkingSpeed;
    }

    void Update()
    {
        if (Input.GetKeyUp(sprint))
        {
            topSpeed = walkingSpeed;
        }

        if (Input.GetKeyDown(sprint))
        {
            topSpeed = sprintingSpeed;
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Gravity and jumping
        if (controller.isGrounded)
        {
            verticalVelocity = -2f; // Small downward force to keep grounded
            if (Input.GetKeyDown(jump))
            {
                verticalVelocity = jumpForce;
            }
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Movement
        Vector3 movement = Vector3.zero;

        if (Input.GetKey(forward))  movement += Vector3.forward;
        if (Input.GetKey(backward)) movement += Vector3.back;
        if (Input.GetKey(left))     movement += Vector3.left;
        if (Input.GetKey(right))    movement += Vector3.right;

        movement = transform.TransformDirection(movement.normalized) * topSpeed;
        movement.y = verticalVelocity;

        controller.Move(movement * Time.deltaTime);

        CameraMovement();
    }

    void CameraMovement()
    {
        rotation.x += Input.GetAxis("Mouse X");
        rotation.y += Input.GetAxis("Mouse Y");
        rotation.y = Mathf.Clamp(rotation.y, -verticalClamp, verticalClamp);

        Quaternion xQuaternion = Quaternion.AngleAxis(rotation.x * Sensitivity, Vector3.up);
        Quaternion yQuaternion = Quaternion.AngleAxis(rotation.y * Sensitivity, Vector3.left);

        transform.localRotation = xQuaternion;
        head.transform.localRotation = yQuaternion;
    }



}