using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public bool justFollow = true;
    public Transform target; // The target GameObject to follow
    public float zoomSpeed = 10.0f; // Speed of zooming in/out
    public float minFOV = 20.0f; // Minimum field of view
    public float maxFOV = 90.0f; // Maximum field of view

    public float rotationSpeedX = 250.0f; // Speed of rotation around X-axis
    public float rotationSpeedY = 120.0f; // Speed of rotation around Y-axis
    public float minYAngle = -20f; // Minimum Y angle
    public float maxYAngle = 80f;  // Maximum Y angle

    private float currentX = 0.0f; // Current X rotation angle
    private float currentY = 20.0f; // Current Y rotation angle
    private bool isFreeCam = false; // Toggle between free cam and follow cam

    private Camera cam; // Reference to the Camera component
    Vector3 smoothPosition = Vector3.zero; // For smooth camera movement
    Vector3 followVelocity = Vector3.zero; // Reference for SmoothDamp
    public float followSmoothTime = 0.05f;   // how “springy” the camera feels
    public Vector3 followOffset   = new Vector3(0, 2.2f, -7f);

    void Start()
    {
        //adjust camera near clipping
        cam.nearClipPlane = 0.01f;
        if (target == null)
        {
            //Debug.LogError("Target not set for CameraController. Please set a target GameObject in the Inspector.");
            enabled = false;
            return;
        }

        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraController requires a Camera component on the same GameObject.");
            enabled = false;
            return;
        }

        // Hide and lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Toggle between free cam and follow cam on pressing 'C'
        if (Input.GetKeyDown(KeyCode.C))
        {
            isFreeCam = !isFreeCam;
            Cursor.lockState = isFreeCam ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isFreeCam;
        }

        // Adjust camera FOV with scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            cam.fieldOfView -= scrollInput * zoomSpeed;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFOV, maxFOV);
        }
        
    }

    void FixedUpdate()
    {
        if (!justFollow) return;

        Vector3 desired = target.position 
                        + target.TransformDirection(followOffset) 
                        + target.GetComponent<Car>().rb.linearVelocity * 0.02f;

        transform.position = Vector3.SmoothDamp(
            transform.position, desired,
            ref followVelocity, followSmoothTime);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(
                target.position + Vector3.up * 1.2f - transform.position + target.GetComponent<Car>().rb.linearVelocity * 0.5f),
            Time.fixedDeltaTime * 10f);
    }

    void LateUpdate()
    {
        if (justFollow)
        {
            return;
        }
        if (isFreeCam)
        {
            FreeCamMode();
        }
        else
        {
            FollowCamMode();
        }
    }

    void FreeCamMode()
    {
        // Free cam movement controls (Arrow keys for movement, right mouse button for looking around)
        float moveSpeed = 10.0f * Time.deltaTime;
        if (Input.GetKey(KeyCode.UpArrow)) transform.position += transform.forward * moveSpeed;
        if (Input.GetKey(KeyCode.DownArrow)) transform.position -= transform.forward * moveSpeed;
        if (Input.GetKey(KeyCode.LeftArrow)) transform.position -= transform.right * moveSpeed;
        if (Input.GetKey(KeyCode.RightArrow)) transform.position += transform.right * moveSpeed;

        // Look around with the mouse when the right mouse button is held
        if (Input.GetMouseButton(1))
        {
            currentX += Input.GetAxis("Mouse X") * rotationSpeedX * Time.deltaTime;
            currentY -= Input.GetAxis("Mouse Y") * rotationSpeedY * Time.deltaTime;
            currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

            transform.rotation = Quaternion.Euler(currentY, currentX, 0);
        }
    }

    void FollowCamMode()
    {
        if (target == null) return;

        // Mouse input for orbiting around the target
        currentX += Input.GetAxis("Mouse X") * rotationSpeedX * Time.deltaTime;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeedY * Time.deltaTime;
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

        // Calculate the desired position and rotation
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = target.position - rotation * Vector3.forward * 10.0f;

        // Update the camera's position and rotation
        transform.position = desiredPosition;
        transform.LookAt(target.position);
    }

    void OnDisable()
    {
        // Show the cursor again when the script is disabled
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}