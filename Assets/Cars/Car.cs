using System;
using UnityEngine;
using System.Collections.Generic;

[Serializable]
public class WheelProperties
{
    [HideInInspector] public TrailRenderer skidTrail;
    [HideInInspector] public GameObject skidTrailGameObject;

    public Vector3 localPosition;
    public float turnAngle = 30f;
    public float suspensionLength = 0.5f;

    [HideInInspector] public float lastSuspensionLength = 0.0f;
    public float mass = 16f; //Wheel mass
    public float size = 0.5f; //Wheel radius used for positioning
    public float engineTorque = 40f; //Torque for each wheel
    public float brakeStrength = 0.5f; //Brakes for each wheel
    public bool sliding = false; //Check if car is sliding
    [HideInInspector] public Vector3 worldSlipDirection;
    [HideInInspector] public Vector3 suspensionForceDirection;
    [HideInInspector] public Vector3 wheelWorldPosition;
    [HideInInspector] public float wheelCircumference;
    [HideInInspector] public float torque = 0.0f;
    [HideInInspector] public GameObject wheelObject;
    [HideInInspector] public Vector3 localVelocity; //Wheel local velocity (?)
    [HideInInspector] public float normalForce; //Ground pushing up on the wheel
    [HideInInspector] public float angularVelocity; //Wheel spin speed rads/sec
    [HideInInspector] public float slip;
    [HideInInspector] public Vector2 input = Vector2.zero;
    [HideInInspector] public float braking = 0;
}

public class Car : MonoBehaviour
{
    public GameObject skidMarkPrefab;
    public float smoothTurn = 0.03f; //Steering smoothing
    public float linearDamping = 0.2f; //Added dampings to simulate reduction in momentum over time (air resistance)
    public float angularDamping = 0.3f; //Rotation damping
    float coefStaticFriction = 2.95f; //Friction when driving
    float coefKineticFriction = 0.85f; //Friction when wheels slide
    public GameObject wheelPrefab;
    public WheelProperties[] wheels;
    public float wheelGripX = 8f; //Sideways grip
    public float wheelGripZ = 42f; //Driving direction grip
    public float suspensionForce = 90f;
    public float dampAmount = 2.5f; //Reduce bounce
    public float suspensionForceClamp = 200f; //Maximum suspension force, prevents unrealistic bounces
    public Rigidbody rb; //physics engine feature
    [HideInInspector] public bool forwards = true;

    public bool steeringAssist = true;
    public bool throttleAssist = true;
    public bool brakeAssist = true;
    [HideInInspector] public Vector2 userInput = Vector2.zero;
    public float downforce = 0.16f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();

        foreach (var w in wheels) //Wheel creation
        {
            w.wheelObject = Instantiate(wheelPrefab, transform);
            w.wheelObject.transform.localPosition = w.localPosition;
            w.wheelObject.transform.eulerAngles = transform.eulerAngles;
            w.wheelObject.transform.localScale = 2f * new Vector3(w.size, w.size, w.size);
            w.wheelCircumference = 2f * Mathf.PI * w.size;

            if (skidMarkPrefab != null) //Skidmark init
            {
                w.skidTrailGameObject = Instantiate(skidMarkPrefab, w.wheelObject.transform);
                w.skidTrailGameObject.transform.localPosition = Vector3.zero;
                w.skidTrailGameObject.transform.localRotation = Quaternion.identity;
                w.skidTrailGameObject.transform.parent = null;

                w.skidTrail = w.skidTrailGameObject.GetComponent<TrailRenderer>();
                if (w.skidTrail != null)
                    w.skidTrail.emitting = false;
            }
        }

        //Physics for stability
        rb.centerOfMass += new Vector3(0, -0.5f, 0);
        rb.inertiaTensor *= 1.4f; //Resistance to rotation
        rb.linearDamping = linearDamping;
        rb.angularDamping = angularDamping;
    }

    void Update()
    {
        float speedFactor = Mathf.Clamp01(rb.linearVelocity.magnitude / 30f); //Speed realtive to max speed
        float steerInputTarget = Input.GetAxisRaw("Horizontal") * Mathf.Lerp(1f, 0.1f, speedFactor * speedFactor); //Steering input, reduced at high speeds
        userInput.x = Mathf.Lerp(userInput.x, steerInputTarget, Mathf.Lerp(0.1f, 0.3f, 1f - speedFactor)); //Smoothly interpolates to new steered target

        userInput.y = Mathf.Lerp(userInput.y, Input.GetAxisRaw("Vertical"), 0.2f); //Throttle/brake and acceleration value (default 0.2)
        bool isBraking = Input.GetKey(KeyCode.S) && forwards;
        if (isBraking) userInput.y = 0; //If brake remove throttle

        float maxSlip = 0;
        for (int i = 0; i < wheels.Length; i++)
            maxSlip = Mathf.Max(maxSlip, wheels[i].slip); //Measures wheel slip

        for (int i = 0; i < wheels.Length; i++)
        {
            if (throttleAssist && maxSlip > 0.96f)
                userInput.y = Mathf.Lerp(userInput.y, 0, maxSlip); //Reduce throttle to reduce wheelspin

            if (steeringAssist && maxSlip > 0.7f)
                userInput.x = Mathf.Lerp(userInput.x, 0, 0.05f); //Steering assist reduces steer angle

            if (maxSlip > 1.0f && wheels[i].localVelocity.magnitude > 0.1f)
            {
                //This is literally traction control, realigns steering to straighten car in extreme slip
                float angle = Mathf.Atan2(wheels[i].localVelocity.x, wheels[i].localVelocity.z) * Mathf.Rad2Deg;
                wheels[i].input = new Vector2(
                    Mathf.Lerp(wheels[i].input.x, Mathf.Clamp(angle / wheels[i].turnAngle, -1f, 1f), 0.1f),
                    wheels[i].input.y
                );
            }

            if (brakeAssist && maxSlip > 0.99f) //Basically real car ABS system
                isBraking = false;

            wheels[i].braking = Mathf.Lerp(wheels[i].braking, (float)(isBraking ? 1 : 0), 0.2f); //Sets inputs to smoothed values
            wheels[i].input = new Vector2(userInput.x, userInput.y);
        }
    }

    void FixedUpdate()
    {
        rb.AddForce(-transform.up * rb.linearVelocity.magnitude * downforce); //Apply downforce

        foreach (var w in wheels)
        {
            RaycastHit hit;
            float rayLen = w.size + w.suspensionLength;
            Transform wheelObj = w.wheelObject.transform;
            Transform wheelVisual = wheelObj.GetChild(0);

            wheelObj.localRotation = Quaternion.Euler(0, w.turnAngle * w.input.x, 0);
            w.wheelWorldPosition = transform.TransformPoint(w.localPosition);
            Vector3 velocityAtWheel = rb.GetPointVelocity(w.wheelWorldPosition);
            w.localVelocity = wheelObj.InverseTransformDirection(velocityAtWheel);
            forwards = w.localVelocity.z > 0.1f;
            w.torque = w.engineTorque * w.input.y;

            float inertia = w.mass * w.size * w.size / 2f;
            float lateralVel = w.localVelocity.x;

            bool grounded = Physics.Raycast( //Check if wheel is grounded
                w.wheelWorldPosition,
                -transform.up,
                out hit,
                rayLen,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore
            );

            Vector3 worldVelAtHit = rb.GetPointVelocity(hit.point);
            float lateralHitVel = wheelObj.InverseTransformDirection(worldVelAtHit).x;

            float lateralFriction = -wheelGripX * lateralVel - 2f * lateralHitVel;
            float longitudinalFriction = -wheelGripZ * (w.localVelocity.z - w.angularVelocity * w.size);

            w.angularVelocity += (w.torque - longitudinalFriction * w.size) / inertia * Time.fixedDeltaTime;
            w.angularVelocity *= 1 - w.braking * w.brakeStrength * Time.fixedDeltaTime;
            if (Input.GetKey(KeyCode.Space))
                w.angularVelocity = 0;

            Vector3 totalLocalForce = new Vector3(lateralFriction, 0f, longitudinalFriction)
                * w.normalForce * coefStaticFriction * Time.fixedDeltaTime;
            float currentMaxFrictionForce = w.normalForce * coefStaticFriction;

            w.sliding = totalLocalForce.magnitude > currentMaxFrictionForce;
            w.slip = totalLocalForce.magnitude / currentMaxFrictionForce;
            totalLocalForce = Vector3.ClampMagnitude(totalLocalForce, currentMaxFrictionForce);
            totalLocalForce *= w.sliding ? (coefKineticFriction / coefStaticFriction) : 1;

            Vector3 totalWorldForce = wheelObj.TransformDirection(totalLocalForce);
            w.worldSlipDirection = totalWorldForce;

            if (grounded)
            {
                float compression = rayLen - hit.distance;
                float damping = (w.lastSuspensionLength - hit.distance) * dampAmount;
                w.normalForce = (compression + damping) * suspensionForce;
                w.normalForce = Mathf.Clamp(w.normalForce, 0f, suspensionForceClamp);

                Vector3 springDir = hit.normal * w.normalForce;
                w.suspensionForceDirection = springDir;

                rb.AddForceAtPosition(springDir + totalWorldForce, hit.point);
                w.lastSuspensionLength = hit.distance;

                //Raise wheel by its radius, only somewhat fixes the floating issue
                wheelObj.position = hit.point + transform.up * w.size;

                if (w.sliding)
                {
                    if (w.skidTrail == null && skidMarkPrefab != null)
                    {
                        GameObject skidTrailObj = Instantiate(skidMarkPrefab, transform);
                        skidTrailObj.transform.SetParent(w.wheelObject.transform);
                        skidTrailObj.transform.localPosition = Vector3.zero;
                        w.skidTrail = skidTrailObj.GetComponent<TrailRenderer>();
                        w.skidTrail.time = 3f;
                        w.skidTrail.autodestruct = true;
                        if (w.skidTrail != null)
                            w.skidTrail.emitting = true;
                    }
                    else if (w.skidTrail != null)
                    {
                        w.skidTrail.emitting = true;
                        w.skidTrail.transform.position = hit.point;

                        Vector3 skidDir = Vector3.ProjectOnPlane(w.worldSlipDirection.normalized, hit.normal);
                        if (skidDir.sqrMagnitude < 0.001f)
                            skidDir = Vector3.ProjectOnPlane(wheelObj.forward, hit.normal).normalized;

                        Quaternion flatRot = Quaternion.LookRotation(skidDir, hit.normal) * Quaternion.Euler(90f, 0f, 0f);
                        w.skidTrail.transform.rotation = flatRot;
                    }
                }
                else if (w.skidTrail != null && w.skidTrail.emitting)
                {
                    w.skidTrail.emitting = false;
                    w.skidTrail.transform.parent = null;
                    Destroy(w.skidTrail.gameObject, w.skidTrail.time);
                    w.skidTrail = null;
                }
            }
            else
            {
                wheelObj.position = w.wheelWorldPosition + transform.up * (w.size - rayLen);
                if (w.skidTrail != null && w.skidTrail.emitting)
                {
                    w.skidTrail.emitting = false;
                    w.skidTrail.transform.parent = null;
                    Destroy(w.skidTrail.gameObject, w.skidTrail.time);
                    w.skidTrail = null;
                }
            }

            wheelVisual.Rotate(
                Vector3.right,
                w.angularVelocity * Mathf.Rad2Deg * Time.fixedDeltaTime,
                Space.Self
            );
        }
    }
}