using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FloatPhysics : MonoBehaviour
{
    public float airDrag = 1f;
    public float waterDrag = 10f;
    public bool affectDirection = true;
    public bool attachToSurface = false;
    public Transform[] floatPoints;

    protected Rigidbody Rigidbody;
    protected Waves Waves;

    protected float waterLine;
    protected Vector3[] waterLinePoints;

    protected Vector3 centerOffset;
    protected Vector3 targetUp;
    protected Vector3 smoothVectorRotation;

    public Vector3 center { get { return centerOffset + transform.position; } }

    void Awake()
    {
        Waves = FindFirstObjectByType<Waves>();
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.useGravity = false;


        waterLinePoints = new Vector3[floatPoints.Length];
        
        for (int i = 0; i < floatPoints.Length; i++)
        {
            waterLinePoints[i] = floatPoints[i].position;
        }
        centerOffset = PhysicsHelper.GetCenter(waterLinePoints) - transform.position;
    }

    void FixedUpdate()
    {
        //default water surface
        var newWaterLine = 0f;
        var pointUnderWater = false;

        //set WaterLinePoints and WaterLine
        for (int i = 0; i < floatPoints.Length; i++)
        {
            //height
            waterLinePoints[i] = floatPoints[i].position;
            waterLinePoints[i].y = Waves.GetHeightFromPoint(floatPoints[i].position);
            newWaterLine += waterLinePoints[i].y / floatPoints.Length;
            if (waterLinePoints[i].y > floatPoints[i].position.y)
                pointUnderWater = true;
        }

        var waterLineDelta = newWaterLine - waterLine;
        waterLine = newWaterLine;

        //compute up vector
        targetUp = PhysicsHelper.GetNormal(waterLinePoints);

        //gravity
        var gravity = Physics.gravity;
        Rigidbody.linearDamping = airDrag;
        if (waterLine > center.y)
        {
            Rigidbody.linearDamping = waterDrag;
            //under water
            if (attachToSurface)
            {
                //attach to water surface
                Rigidbody.position = new Vector3(Rigidbody.position.x, waterLine - centerOffset.y, Rigidbody.position.z);
            }
            else
            {
                //go up
                gravity = affectDirection ? targetUp * -Physics.gravity.y : -Physics.gravity;
                transform.Translate(Vector3.up * waterLineDelta * 0.9f);
            }
        }
        Rigidbody.AddForce(gravity * Mathf.Clamp(Mathf.Abs(waterLine - center.y),0,1));

        //rotation
        if (pointUnderWater)
        {
            //attach to water surface
            targetUp = Vector3.SmoothDamp(transform.up, targetUp, ref smoothVectorRotation, 0.2f);
            Rigidbody.rotation = Quaternion.FromToRotation(transform.up, targetUp) * Rigidbody.rotation;
        }

    }
}
