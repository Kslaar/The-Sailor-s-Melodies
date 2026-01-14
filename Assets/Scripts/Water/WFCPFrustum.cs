using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class WFCPFrustum : MonoBehaviour
{
    public Camera cam;
    public float waterY = 0f;

    [Tooltip("Welcher Punkt im Bild wird als Referenz genommen. (0.5,0.5)=Mitte")]
    public Vector2 viewportAnchor = new Vector2(0.5f, 0.5f);

    [Tooltip("Optional: Positionsupdate in großen Schritten. 0 = kontinuierlich.")]
    public float stepSize = 0f;

    // wird beim Start automatisch ermittelt:
    private Vector3 worldOffset;

    // OceanOffset für smoothes Pattern
    private Vector3 lastPosition;
    private Vector2 oceanOffset;
    private Renderer rend;
    private MaterialPropertyBlock mpb;

    void Start()
    {
        if (!cam) cam = Camera.main;

        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();

        // Anchor-Punkt auf Wasser-Ebene
        Vector3 anchor = ProjectViewportToPlane(viewportAnchor);

        // Der Offset ist: "wo steht mein Wasser jetzt" minus "wo wäre der Anchor"
        // => genau dein manuell eingestellter Down-Left Shift bleibt erhalten.
        worldOffset = transform.position - anchor;

        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        if (!cam) return;

        Vector3 anchor = ProjectViewportToPlane(viewportAnchor);

        Vector3 newPos = new Vector3(anchor.x, waterY, anchor.z) + new Vector3(worldOffset.x, 0f, worldOffset.z);

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

    Vector3 ProjectViewportToPlane(Vector2 vp)
    {
        Ray r = cam.ViewportPointToRay(new Vector3(vp.x, vp.y, 0f));
        float t = (waterY - r.origin.y) / r.direction.y;
        return r.origin + r.direction * t;
    }
}
