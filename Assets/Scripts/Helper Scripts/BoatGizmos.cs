using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BoatGizmos : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BoatControl controller;
    [Header("Draw")]
    [SerializeField] private float velocityScale = 0.5f;
    [SerializeField] private float angularScale = 1.0f;
    [SerializeField] private float arrowHead = 0.25f;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        controller = GetComponent<BoatControl>();
    }

    private void OnDrawGizmos()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        Vector3 p = rb.worldCenterOfMass;

        // 1) Vorwärtsrichtung (Boot heading)
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(p, p + transform.forward * 2f);

        // 2) Velocity (Bewegungsrichtung)
        Gizmos.color = Color.green;
        DrawArrow(p, rb.linearVelocity * velocityScale);

        // 3) Lateral Velocity (seitliches Rutschen)
        Vector3 lateral = Vector3.Project(rb.linearVelocity, transform.right);
        Gizmos.color = new Color(1f, 0.5f, 0f); // orange
        DrawArrow(p, lateral * velocityScale);

        // 4) Yaw-Rate (Drehgeschwindigkeit um Up)
        float yaw = Vector3.Dot(rb.angularVelocity, transform.up); // rad/s
        Gizmos.color = Color.magenta;
        DrawArrow(p, transform.right * (yaw * angularScale));

        // 5) Schwerpunkt
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(p, 0.12f);
    }

    private void DrawArrow(Vector3 origin, Vector3 vec)
    {
        Vector3 end = origin + vec;
        Gizmos.DrawLine(origin, end);

        if (vec.sqrMagnitude < 0.0001f) return;

        Vector3 dir = vec.normalized;
        Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 160, 0) * Vector3.forward;
        Vector3 left  = Quaternion.LookRotation(dir) * Quaternion.Euler(0, 200, 0) * Vector3.forward;

        Gizmos.DrawLine(end, end + right * arrowHead);
        Gizmos.DrawLine(end, end + left * arrowHead);
    }
}
