using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Drawing;

[RequireComponent(typeof(Rigidbody))]
public class BoatControl : MonoBehaviour
{

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

    private readonly Dictionary<object, float> speedMods = new();
    private readonly Dictionary<object, float> thrustMods = new();
    private readonly Dictionary<object, float> turnMods = new();

    private int boostCount = 0;
    public bool boostActive => boostCount > 0;
    // public bool boostActive { get; private set; }
    // public void SetBoostActive(bool active) => boostActive = active;
    // public float thrustMultiplier { get; set; } = 1f;
    // public float turnMultiplier { get; set; } = 1f;
    // public float speedMultiplier { get; set; } = 1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 3f;
        

    }

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

    // Schnittstelle für die Fähigkeiten aus anderen Scripts
    public void SetSpeedMultiplier(object source, float multiplier) => SetMod(speedMods, source, multiplier);
    public void SetThrustMultiplier(object source, float multiplier) => SetMod(thrustMods, source, multiplier);
    public void SetTurnMultiplier(object source, float multiplier) => SetMod(turnMods, source, multiplier);

    public void ClearMultipliers(object source)
    {
        speedMods.Remove(source);
        thrustMods.Remove(source);
        turnMods.Remove(source);
    }

    public void PushBoost(object source)
    {
        boostCount++;
    }

    public void ReductBoost(object source)
    {
        boostCount = Mathf.Max(0, boostCount - 1);
    }

    private void ApplyThrottle(float input)
    {
        if (Mathf.Approximately(input, 0f)) return;

        Vector3 forwardFlat = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;

        float baseSpeed = (input > 0f ? maxForwardSpeed : maxReverseSpeed) * Mathf.Sign(input);
        float targetSpeed = baseSpeed * GetProduct(speedMods);

        float baseForce = input > 0f ? accelerationForce : reverseAccelerationForce;
        float force = baseForce * GetProduct(thrustMods);

        PhysicsHelper.ApplyForceToReachVelocity(rb, forwardFlat * targetSpeed, force);
        
    }

    private void ApplySteering(float input)
    {
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float speedAbs = Mathf.Abs(forwardSpeed);

        float minSteerFactor = 0.75f;
        float speedSteerFactor = Mathf.Clamp01(speedAbs / minSpeedForFullSteering); 
        float steerFactor = Mathf.Lerp(minSteerFactor, 1f, speedSteerFactor);

        float desiredYawDeg = input * maxYawDegPerSec * steerFactor * GetProduct(turnMods);
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
    
    public void SetMod(Dictionary<object, float> dict, object source, float multiplier)
    {
        if (multiplier <= 0f) multiplier = 0.0001f;
        dict[source] = multiplier;
    }

    private static float GetProduct(Dictionary<object, float> dict)
    {
        float product = 1f;
        foreach (var keyValue in dict) product *= keyValue.Value;
        return product;
    }
}
