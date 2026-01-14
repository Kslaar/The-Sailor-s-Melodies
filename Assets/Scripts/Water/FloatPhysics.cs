using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FloatPhysics : MonoBehaviour
{
    [Header("Drag")]
    public float airDrag = 1f;
    public float waterDrag = 10f;

    [Header("Buoyancy")]
    public bool affectDirection = true;
    public bool attachToSurface = false;
    public Transform[] floatPoints;

    [Header("Stabilization")]
    [Tooltip("Wie schnell sich das Boot an die Wasseroberfläche anpasst (Pitch/Roll). Höher = snappier.")]
    [Range(0.01f, 1f)] public float rotationLerp = 0.2f;

    [Tooltip("Glättungszeit für Up-Vektor (Pitch/Roll).")]
    [Range(0.01f, 1f)] public float upSmoothTime = 0.2f;

    [Tooltip("Multiplikator für die vertikale Anpassung an die Wasserlinie.")]
    [Range(0f, 2f)] public float verticalFollow = 0.9f;

    protected Rigidbody Rigidbody;
    protected Waves Waves;

    protected float waterLine;
    protected Vector3[] waterLinePoints;

    protected Vector3 centerOffset;
    protected Vector3 targetUp;
    protected Vector3 smoothVectorRotation;

    public Vector3 center => centerOffset + transform.position;

    void Awake()
    {
        Waves = FindFirstObjectByType<Waves>();
        Rigidbody = GetComponent<Rigidbody>();
        Rigidbody.useGravity = false;

        waterLinePoints = new Vector3[floatPoints.Length];

        for (int i = 0; i < floatPoints.Length; i++)
            waterLinePoints[i] = floatPoints[i].position;

        centerOffset = PhysicsHelper.GetCenter(waterLinePoints) - transform.position;
    }

    void FixedUpdate()
    {
        // --- 1) Wasserlinie berechnen + Unterwasser-Anteil (für smooth Drag) ---
        float newWaterLine = 0f;
        int underCount = 0;

        for (int i = 0; i < floatPoints.Length; i++)
        {
            Vector3 p = floatPoints[i].position;

            waterLinePoints[i] = p;
            waterLinePoints[i].y = Waves.GetHeightFromPoint(p);

            newWaterLine += waterLinePoints[i].y / floatPoints.Length;

            if (waterLinePoints[i].y > p.y)
                underCount++;
        }

        float waterLineDelta = newWaterLine - waterLine;
        waterLine = newWaterLine;

        // 0..1: wie viel vom Boot "im Wasser" ist (basierend auf FloatPoints)
        float submersion = (float)underCount / floatPoints.Length;

        // --- 2) Ziel-Up aus Wasseroberfläche (für Pitch/Roll) ---
        targetUp = PhysicsHelper.GetNormal(waterLinePoints);

        // --- 3) Drag smooth statt hart umschalten (verhindert Speed-Pulsieren) ---
        Rigidbody.linearDamping = Mathf.Lerp(airDrag, waterDrag, submersion);

        // --- 4) Auftrieb/Gravity ---
        Vector3 gravity = Physics.gravity;

        // Nur wenn zumindest ein Punkt unter Wasser ist, beeinflussen wir die Auftriebsrichtung/Position
        if (submersion > 0f)
        {
            if (attachToSurface)
            {
                // an die Wasseroberfläche "kleben"
                Rigidbody.position = new Vector3(
                    Rigidbody.position.x,
                    waterLine - centerOffset.y,
                    Rigidbody.position.z
                );
            }
            else
            {
                // "Auftrieb" in Richtung targetUp (wenn affectDirection)
                gravity = affectDirection ? targetUp * -Physics.gravity.y : -Physics.gravity;

                Rigidbody.MovePosition(Rigidbody.position + Vector3.up * waterLineDelta * verticalFollow);
            }
        }

        // Stärke abhängig davon wie weit Center von Wasserlinie weg ist (geclamped)
        Rigidbody.AddForce(gravity * Mathf.Clamp(Mathf.Abs(waterLine - center.y), 0f, 1f));

        // --- 5) Rotation: Pitch/Roll an Wasser anpassen, Yaw nicht kaputt machen ---
        if (underCount > 0)
        {
            // Up-Vektor glätten
            targetUp = Vector3.SmoothDamp(transform.up, targetUp, ref smoothVectorRotation, upSmoothTime);

            // Forward auf Wasserfläche projizieren => stabiler LookRotation, Yaw bleibt "natürlich"
            Vector3 fwd = Vector3.ProjectOnPlane(transform.forward, targetUp);
            if (fwd.sqrMagnitude < 0.0001f)
                fwd = Vector3.ProjectOnPlane(transform.forward, Vector3.up);

            Quaternion targetRot = Quaternion.LookRotation(fwd.normalized, targetUp);

            Rigidbody.MoveRotation(Quaternion.Slerp(Rigidbody.rotation, targetRot, rotationLerp));
        }
    }
}
