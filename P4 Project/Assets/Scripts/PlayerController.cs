using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private GameObject head;

    [Header("Camera")] public float Sensitivity;
    public float verticalClamp;
    private Vector2 rotation = Vector2.zero;
    
    [Header("Movement")] public float acceleration;
    public float topSpeed;
    
    [Header("Keys")] public KeyCode forward;
    public KeyCode backward, left, right;

    private Vector3 movement;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        head = transform.GetChild(0).gameObject;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
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
        if (movement != Vector3.zero)
            rb.AddForce(acceleration * Time.deltaTime * 100 * transform.TransformDirection(movement), ForceMode.Impulse);
        if (rb.linearVelocity.magnitude > topSpeed)
        {
            float temp = rb.linearVelocity.magnitude - topSpeed;
            //Vector3 tempVector = temp * -1 * rb.linearVelocity.normalized;
            //Debug.Log("TOP SPEED: " + rb.linearVelocity.magnitude + " / SPEED OVER LIMIT: " + temp + "FORCE ADDED: (" + tempVector.x + ", " + tempVector.y + ", " + tempVector.z + ")");
            rb.AddForce(temp * -1 * rb.linearVelocity.normalized);
        }
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
