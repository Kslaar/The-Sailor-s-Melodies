using UnityEngine;

public class WaterFollowCameraPatch : MonoBehaviour
{
    public Camera cam;
    public float waterY = 0f;

    [Tooltip("Viewport-Zentrum, das du abdecken willst. (0.5,0.5) = Bildmitte. Für mehr Wasser oben z.B. (0.5,0.6)")]
    public Vector2 viewportAnchor = new Vector2(0.5f, 0.6f);

    [Tooltip("Optional: Follow in großen Schritten (damit selten verschoben wird). 0 = immer exakt.")]
    public float stepSize = 0f;

    private Vector3 lastPosition;
    private Vector2 oceanOffset;
    private Renderer rend;
    private MaterialPropertyBlock mpb;

    void Start()
    {
        if (!cam) cam = Camera.main;

        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();
        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        if (!cam) return;

        // Ray vom gewünschten Viewport-Punkt auf die Wasser-Ebene y = waterY
        Ray r = cam.ViewportPointToRay(new Vector3(viewportAnchor.x, viewportAnchor.y, 0f));
        float t = (waterY - r.origin.y) / r.direction.y;
        Vector3 anchorWorld = r.origin + r.direction * t;

        Vector3 newPos = new Vector3(anchorWorld.x, waterY, anchorWorld.z);

        if (stepSize > 0f)
        {
            newPos.x = Mathf.Round(newPos.x / stepSize) * stepSize;
            newPos.z = Mathf.Round(newPos.z / stepSize) * stepSize;
        }

        Vector3 delta = newPos - lastPosition;
        oceanOffset += new Vector2(delta.x, delta.z);

        transform.position = newPos;
        lastPosition = newPos;

        rend.GetPropertyBlock(mpb);
        mpb.SetVector("_OceanOffset", oceanOffset);
        rend.SetPropertyBlock(mpb);
    }
}
