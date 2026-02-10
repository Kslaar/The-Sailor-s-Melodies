using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ObstacleAvoidance : MonoBehaviour
{
    [Header("Refs")]
    public Collider avoidanceCollider;

    [Header("Detection")]
    public LayerMask obstacleMask;
    public float heightThreshold = -2f;

    // [Tooltip("Mindestabstand zum Obstacle")]
    // public float minDistance = 3f;

    [Tooltip("Ab welcher Distanz fangen wir an zu reagieren")]
    public float detectPadding = 2f;

    [Header("Feel")]
    public float pushStrength = 18f;
    public float slideStrength = 8f;
    public float maxAccel = 12f;
    public float maxCorrectionPerStep = 0.08f;
    // public float centerYOffset = 0f;

    Rigidbody rb;
    readonly Collider[] hits = new Collider[64];

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (!avoidanceCollider) avoidanceCollider = GetComponent<Collider>();
    }

    void FixedUpdate()
    {
        if (RaceManager.Instance != null && RaceManager.Instance.IsRacing) return;
        if (!avoidanceCollider) return;

        Vector3 center = avoidanceCollider.bounds.center;
        float radius = Mathf.Max(avoidanceCollider.bounds.extents.x, avoidanceCollider.bounds.extents.z) + detectPadding;

        int count = Physics.OverlapSphereNonAlloc(center, radius, hits, obstacleMask, QueryTriggerInteraction.Ignore);
        if (count <= 0) return;

        Vector3 vel = rb.linearVelocity;

        // Problem mit dieser Spielmechanik könnte manchaml festklemmen (zu viele zu nahe Obstacles), daher nur das nächste Hindernis!
        float bestDist = 0f;
        Vector3 bestDir = Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            var col = hits[i];
            if (!col) continue;
            if (col.bounds.max.y <= heightThreshold) continue;

            if (Physics.ComputePenetration(
                    avoidanceCollider, avoidanceCollider.transform.position, avoidanceCollider.transform.rotation,
                    col, col.transform.position, col.transform.rotation,
                    out Vector3 dir, out float dist))
            {
                dir.y = 0f;
                float dirLen = dir.magnitude;
                if (dirLen < 0.0001f) continue;
                dir /= dirLen;

                if (dist > bestDist)
                {
                    bestDist = dist;
                    bestDir = dir;
                }
            }        
        }

        if (bestDist <= 0f) return;

        float corr = Mathf.Min(maxCorrectionPerStep, bestDist);
        rb.MovePosition(rb.position + bestDir * corr);

        float accel = Mathf.Min(maxAccel, pushStrength * bestDist);
        rb.AddForce(bestDir * accel, ForceMode.Acceleration);

        Vector3 into = Vector3.Project(vel, -bestDir);
        rb.AddForce(-into *slideStrength, ForceMode.Acceleration);
    }
}
