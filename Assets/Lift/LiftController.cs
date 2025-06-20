using UnityEngine;

public class LiftController : MonoBehaviour
{
    [Header("Platform Settings")]
    public Transform platform;
    public float platformLowerDistance = 3f;
    public float platformMoveDuration = 2f;

    [Header("Arms Settings")]
    public Transform arm1;
    public Transform arm2;
    public float armLowerDistance = 1.5f;
    public float armRotationAngle = 45f;
    public float armsMoveDuration = 3f;

    private Vector3 platformStartPos; //Start positions
    private Vector3 platformTargetPos;

    private Vector3 arm1StartPos; //Start positions
    private Vector3 arm2StartPos;
    private Vector3 arm1TargetPos;
    private Vector3 arm2TargetPos;

    private Quaternion arm1StartRot; //Start rotations
    private Quaternion arm2StartRot;
    private Quaternion arm1TargetRot;
    private Quaternion arm2TargetRot;

    private float platformMoveProgress = 0f;
    private float armsMoveProgress = 0f;

    private bool isPlatformMoving = false;
    private bool areArmsMoving = false;

    void Start()
    {
        //Set start and target positions for platform
        platformStartPos = platform.position;
        platformTargetPos = platformStartPos - new Vector3(0, platformLowerDistance, 0);

        //Set start and target positions for arms
        arm1StartPos = arm1.position;
        arm2StartPos = arm2.position;
        arm1TargetPos = arm1StartPos - new Vector3(0, armLowerDistance, 0);
        arm2TargetPos = arm2StartPos - new Vector3(0, armLowerDistance, 0);

        //Set start and target rotations for arms
        arm1StartRot = arm1.localRotation;
        arm2StartRot = arm2.localRotation;
        arm1TargetRot = arm1StartRot * Quaternion.Euler(armRotationAngle, 0, 0);
        arm2TargetRot = arm2StartRot * Quaternion.Euler(-armRotationAngle, 0, 0);
    }

    void Update()
    {
        if (isPlatformMoving)
        {
            platformMoveProgress += Time.deltaTime / platformMoveDuration;
            platformMoveProgress = Mathf.Clamp01(platformMoveProgress);

            float eased = Mathf.SmoothStep(0f, 1f, platformMoveProgress);
            platform.position = Vector3.Lerp(platformStartPos, platformTargetPos, eased);

            if (platformMoveProgress >= 1f)
                isPlatformMoving = false;
        }

        if (areArmsMoving)
        {
            armsMoveProgress += Time.deltaTime / armsMoveDuration;
            armsMoveProgress = Mathf.Clamp01(armsMoveProgress);

            float eased = Mathf.SmoothStep(0f, 1f, armsMoveProgress);
            arm1.position = Vector3.Lerp(arm1StartPos, arm1TargetPos, eased);
            arm2.position = Vector3.Lerp(arm2StartPos, arm2TargetPos, eased);

            arm1.localRotation = Quaternion.Slerp(arm1StartRot, arm1TargetRot, eased);
            arm2.localRotation = Quaternion.Slerp(arm2StartRot, arm2TargetRot, eased);

            if (armsMoveProgress >= 1f)
                areArmsMoving = false;
        }
    }

    //Called from game manager
    public void StartLowering()
    {
        platformMoveProgress = 0f;
        armsMoveProgress = 0f;

        isPlatformMoving = true;
        areArmsMoving = true;
    }
}