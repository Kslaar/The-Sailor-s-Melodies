using UnityEngine;

public class WaterPatchFollow : MonoBehaviour
{
    public Transform target;
    public float waterY = 0f;
    public float snapSize = 20f;

    private Vector3 lastPosition;
    private Vector2 oceanOffset;

    private Vector3 initialOffset;

    private Renderer rend;
    private MaterialPropertyBlock mpb;

    void Start()
    {
        rend = GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();

        if (target) initialOffset = transform.position - target.position;

        lastPosition = transform.position;
    }

    void LateUpdate()
    {
        if (!target) return;

        float x = target.position.x + initialOffset.x;
        float z = target.position.z + initialOffset.z;

        if (snapSize > 0f)
        {
            x = Mathf.Round(x / snapSize) * snapSize;
            z = Mathf.Round(z / snapSize) * snapSize;
        }

        Vector3 newPos = new Vector3(x, waterY, z);

        // Delta berechnen
        Vector3 delta = newPos - lastPosition;

        // Offset akkumulieren (X/Z)
        oceanOffset += new Vector2(delta.x, delta.z);

        transform.position = newPos;
        lastPosition = newPos;

        // Offset an Shader schicken
        rend.GetPropertyBlock(mpb);
        mpb.SetVector("_OceanOffset", oceanOffset);
        rend.SetPropertyBlock(mpb);
    }
}
