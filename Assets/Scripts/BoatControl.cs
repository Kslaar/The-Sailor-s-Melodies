using UnityEngine;
using UnityEngine.InputSystem;

public class BoatControl : MonoBehaviour
{
    public Transform engine;
    public float steerPower;
    public float enginePower;
    public float maxSpeed;
    public float drag;

    protected Rigidbody Rigidbody;
    protected Quaternion startRotation;
    void Start()
    {
        Rigidbody = GetComponent<Rigidbody>();
        startRotation = engine.localRotation;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        var forceDirection = transform.forward;
        var steer = 0;

        if (Keyboard.current.aKey.isPressed) steer = 1;
        if (Keyboard.current.dKey.isPressed) steer = -1;  

        Rigidbody.AddForceAtPosition(steer * transform.right * steerPower / 100f, engine.position); 

        var forward = Vector3.Scale(new Vector3(1, 0, 1), transform.forward);

        if (Keyboard.current.wKey.isPressed) 
            PhysicsHelper.ApplyForceToReachVelocity(Rigidbody, forward * maxSpeed, enginePower);
        if (Keyboard.current.sKey.isPressed)
            PhysicsHelper.ApplyForceToReachVelocity(Rigidbody, forward * -maxSpeed, enginePower);
    }
}
