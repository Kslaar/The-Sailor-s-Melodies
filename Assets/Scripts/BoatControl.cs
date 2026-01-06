using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class BoatControl : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform engine;

    [Header("Throttle")]
    [SerializeField] private float maxForwardSpeed = 6f;
    [SerializeField] private float maxReverseSpeed = 3f;
    [SerializeField] private float accelerationForce = 8f;
    [SerializeField] private float reverseAccelerationForce = 6f;
    
    [Header("Steering")]
    [SerializeField] private float maxYawDegPerSec = 35f; // Wir setzen ein Rotationsgeschwindigkeits-Cap!!
    [SerializeField] private float steerResponsivness = 6f; // Wie schnell die Zielrate erreicht wird
    [SerializeField] private float yawDamping = 2.5f;

    [Header("Water feel")]
    [SerializeField] private float lateralDrag = 2f;
    [SerializeField] private float minSpeedForFullSteering = 2f;

    private Rigidbody rb;
    //protected Rigidbody Rigidbody;
    // protected Quaternion startRotation;

    public float ThrustMultiplier { get; set; } = 1f;
    public float TurnMultiplier { get; set; } = 1f;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 3f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float steerInput = 0f;

        if (Keyboard.current.aKey.isPressed) steerInput += 1;
        if (Keyboard.current.dKey.isPressed) steerInput -= 1;  

        float throttleInput = 0f;

        if (Keyboard.current.wKey.isPressed) throttleInput += 1f;
        if (Keyboard.current.sKey.isPressed) throttleInput -= 1f;

        ApplyThrottle(throttleInput);
        ApplySteering(steerInput);
        ApplyHydroDrag();
    }

    private void ApplyThrottle(float input)
    {
        if (Mathf.Approximately(input, 0f)) return;

        Vector3 forwardFlat = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        float targetSpeed = (input > 0f ? maxForwardSpeed : maxReverseSpeed) * Mathf.Sign(input);
        float force = (input > 0f ? accelerationForce : reverseAccelerationForce) * ThrustMultiplier;

        PhysicsHelper.ApplyForceToReachVelocity(rb, forwardFlat * targetSpeed, force);
    }

    private void ApplySteering(float input)
    {
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float speedAbs = Mathf.Abs(forwardSpeed);

        float minSteerFactor = 0.75f;
        float speedSteerFactor = Mathf.Clamp01(speedAbs / minSpeedForFullSteering); 
        float steerFactor = Mathf.Lerp(minSteerFactor, 1f, speedSteerFactor);

        float desiredYawDeg = input * maxYawDegPerSec * steerFactor * TurnMultiplier;
        float desiredYawRad = desiredYawDeg * Mathf.Deg2Rad;

        float currentYaw = Vector3.Dot(rb.angularVelocity, transform.up);

        float error = desiredYawRad - currentYaw;
        float damping = yawDamping * (Mathf.Abs(input) < 0.01f ? 2.5f : 1f);
        float torque = error * steerResponsivness - currentYaw * damping;

        rb.AddTorque(transform.up * torque, ForceMode.Acceleration);
    }

    private void ApplyHydroDrag()
    {
        Vector3 lateral = Vector3.Project(rb.linearVelocity, transform.right);
        rb.AddForce(-lateral * lateralDrag, ForceMode.Acceleration);
    }

    ////////////////////////////////////////////////////
    
    public void AddImpulse(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
    }

    public void SetThrustMultiplier(float value)
    {
        ThrustMultiplier = value;
    }
}
