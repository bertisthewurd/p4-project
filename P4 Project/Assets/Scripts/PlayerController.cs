using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private GameObject head;

    [Header("Camera")] public float Sensitivity;
    public float verticalClamp;
    private Vector2 rotation = Vector2.zero;
    
    [Header("Movement")] public float acceleration;
    public float walkingSpeed, sprintingSpeed;
    
    [Header("Jumping")] public float jumpForce;
    public float rayLength;
    
    [Header("Keys")] public KeyCode forward;
    public KeyCode backward, left, right, sprint, jump;

    private Vector3 movement;
    private float topSpeed;
    private bool isGrounded;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
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
                Debug.Log("Walking");
        }

        if (Input.GetKeyDown(sprint))
        {
            topSpeed = sprintingSpeed;
            Debug.Log("Sprinting");
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

        CheckGround();
        
        if (Input.GetKeyDown(jump) && isGrounded)
        {
            InitiateJump();
        }
        
        CameraMovement();
        
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        movement = Vector3.zero;

        if (Input.GetKey(forward))
        {
            movement += Vector3.forward;
        }

        if (Input.GetKey(backward))
        {
            movement += Vector3.back;
        }

        if (Input.GetKey(left))
        {
            movement += Vector3.left;
        }

        if (Input.GetKey(right))
        {
            movement += Vector3.right;
        }
        
        //Thing about translating movementvector into velocity here
        ApplyMovement();
    }

    void ApplyMovement()
    {
        if (rb.linearVelocity.magnitude < topSpeed)
        {
            if (movement != Vector3.zero)
                rb.AddForce(acceleration * transform.TransformDirection(movement), ForceMode.Acceleration);
        }
        
        //Debug.Log("Velocity: " + rb.linearVelocity.magnitude);
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

    void CheckGround()
    {
        if (Physics.Raycast(transform.position, Vector3.down, rayLength))
        {
            //Debug.Log("Grounded");
            isGrounded = true;
        }
        else
        {
            //Debug.Log("Not Grounded");
            isGrounded = false;
        }
    }

    void InitiateJump()
    {
        rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
    }
}
