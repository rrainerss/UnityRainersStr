using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; //Moving target
    public Transform fixedCameraPoint; //Fixed start
    public float transitionDuration = 2f; //Animation duration (s)

    private float transitionProgress = 0f;
    private bool transitioning = true; //These bools switch between each other
    private bool justFollow = false;

    public Vector3 followOffset = new Vector3(0, 2.2f, -7f);
    private Vector3 followVelocity = Vector3.zero; //"Damping"??
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
            Debug.LogError("References missing"); //Disable script if not setup
            enabled = false;
            return;
        }

        //Start at fixed position and rotation
        transform.position = fixedCameraPoint.position;
        transform.rotation = fixedCameraPoint.rotation;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    //Once per frame loop
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

    //FPS independent loop
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
            Vector3 pos = Vector3.Lerp(fixedCameraPoint.position, followPos, transitionProgress); //Interpolation between points

            Vector3 lookTarget = target.position + Vector3.up * 1.2f;
            Quaternion rotStart = fixedCameraPoint.rotation;
            Quaternion rotEnd = Quaternion.LookRotation(lookTarget - pos);
            Quaternion rot = Quaternion.Slerp(rotStart, rotEnd, transitionProgress); //Interpolates rotation between the points

            transform.position = pos;
            transform.rotation = rot;
        }

        else if (justFollow)
        {
            //Just follow the car once transition is done
            Vector3 desiredPos = target.position + target.TransformDirection(followOffset);

            Rigidbody rb = target.GetComponent<Rigidbody>();
            Vector3 velocityOffset = rb != null ? rb.linearVelocity * 0.02f : Vector3.zero; //Physics feature! (technically)

            transform.position = Vector3.SmoothDamp(transform.position, desiredPos + velocityOffset, ref followVelocity, followSmoothTime);

            Vector3 lookTarget = target.position + Vector3.up * 1.2f + velocityOffset * 0.5f;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookTarget - transform.position), Time.fixedDeltaTime * 10f);
        }
    }
}
