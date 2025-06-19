using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Transform fixedCameraPoint;
    public float transitionDuration = 2f;

    private float transitionProgress = 0f;
    private bool transitioning = true;
    private bool justFollow = false;

    public Vector3 followOffset = new Vector3(0, 2.2f, -7f);
    private Vector3 followVelocity = Vector3.zero;
    public float followSmoothTime = 0.05f;

    private Camera cam;
    public float zoomSpeed = 10f;
    public float minFOV = 20f;
    public float maxFOV = 90f;

    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null || target == null || fixedCameraPoint == null)
        {
            Debug.LogError("Camera, target, or fixedCameraPoint missing!");
            enabled = false;
            return;
        }

        //Start at fixed position and rotation
        transform.position = fixedCameraPoint.position;
        transform.rotation = fixedCameraPoint.rotation;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        //Zoom control
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            cam.fieldOfView -= scrollInput * zoomSpeed;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFOV, maxFOV);
        }
    }

    void FixedUpdate()
    {
        if (transitioning)
        {
            transitionProgress += Time.fixedDeltaTime / transitionDuration;
            if (transitionProgress >= 1f)
            {
                transitionProgress = 1f;
                transitioning = false;
                justFollow = true;
            }

            Vector3 followPos = target.position + target.TransformDirection(followOffset);
            Vector3 pos = Vector3.Lerp(fixedCameraPoint.position, followPos, transitionProgress);

            Vector3 lookTarget = target.position + Vector3.up * 1.2f;
            Quaternion rotStart = fixedCameraPoint.rotation;
            Quaternion rotEnd = Quaternion.LookRotation(lookTarget - pos);
            Quaternion rot = Quaternion.Slerp(rotStart, rotEnd, transitionProgress);

            transform.position = pos;
            transform.rotation = rot;
        }
        else if (justFollow)
        {
            //Smooth follow 
            Vector3 desiredPos = target.position + target.TransformDirection(followOffset);

            Rigidbody rb = target.GetComponent<Rigidbody>();
            Vector3 velocityOffset = rb != null ? rb.linearVelocity * 0.02f : Vector3.zero;

            transform.position = Vector3.SmoothDamp(transform.position, desiredPos + velocityOffset, ref followVelocity, followSmoothTime);

            Vector3 lookTarget = target.position + Vector3.up * 1.2f + velocityOffset * 0.5f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookTarget - transform.position), Time.fixedDeltaTime * 10f);
        }
    }
}
