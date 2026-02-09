using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Waves : MonoBehaviour
{
    [Header("Visual Mesh (optional)")]
    public bool generateMesh = true;
    public float size = 50f;        // Weltgröße des sichtbaren Wassers (nur Optik)
    public int resolution = 64;     // Mesh-Auflösung (64 => 65x65 Vertices)

    [Header("Wave Settings (Physik + Optik)")]
    public Octave[] octaves;

    MeshFilter meshFilter;
    Mesh mesh;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;

    int dim;     // resolution
    int dim1;    // resolution + 1

    void Awake()
    {
        if (!generateMesh) return;

        dim = Mathf.Max(2, resolution);
        dim1 = dim + 1;

        mesh = new Mesh();
        mesh.name = "WavesMesh";
        mesh.MarkDynamic();

        meshFilter = GetComponent<MeshFilter>();
        if (!meshFilter) meshFilter = gameObject.AddComponent<MeshFilter>();

        var mr = GetComponent<MeshRenderer>();
        if (!mr) gameObject.AddComponent<MeshRenderer>();

        vertices = GenerateVertices();
        triangles = GenerateTriangles();
        uvs = GenerateUVs();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;
    }

    void Update()
    {
        if (!generateMesh || mesh == null) return;

        // Update Visual Mesh using the SAME world sampling (seamless, chunk-friendly)
        for (int x = 0; x <= dim; x++)
        {
            for (int z = 0; z <= dim; z++)
            {
                int idx = Index(x, z);
                Vector3 p = vertices[idx];

                // local -> world
                Vector3 world = transform.TransformPoint(p);
                float y = SampleHeightWorld(world.x, world.z, Time.time);

                // world height -> local (Y only)
                // (Wir setzen nur Y, X/Z bleiben local)
                p.y = y - transform.position.y;
                vertices[idx] = p;
            }
        }

        mesh.SetVertices(vertices);
        mesh.RecalculateNormals();
    }

    /// <summary>Physik-Höhe: funktioniert überall in der Welt (kein Clamp, keine Mesh-Fläche nötig).</summary>
    public float GetHeightFromPoint(Vector3 worldPos)
    {
        return SampleHeightWorld(worldPos.x, worldPos.z, Time.time);
    }

    float SampleHeightWorld(float wx, float wz, float t)
    {
        float y = 0f;

        // Skalen: du willst typischerweise "world units" sinnvoll mappen.
        // Hier: scale ist direkt "Frequenz" in Worldspace (kleiner => größere Wellen).
        for (int o = 0; o < octaves.Length; o++)
        {
            var oc = octaves[o];

            if (oc.alternate)
            {
                float n = Mathf.PerlinNoise(wx * oc.scale.x, wz * oc.scale.y) * Mathf.PI * 2f;
                y += Mathf.Cos(n + oc.speed.magnitude * t) * oc.height;
            }
            else
            {
                float n = Mathf.PerlinNoise(
                            wx * oc.scale.x + t * oc.speed.x,
                            wz * oc.scale.y + t * oc.speed.y
                          ) - 0.5f;
                y += n * oc.height;
            }
        }

        // in Worldspace ist y direkt “Höhe”
        return transform.position.y + y * transform.lossyScale.y;
    }

    Vector3[] GenerateVertices()
    {
        // Zentriert um 0: von -size/2 .. +size/2 (viel angenehmer für Tiling)
        var v = new Vector3[dim1 * dim1];
        float half = size * 0.5f;

        for (int x = 0; x <= dim; x++)
        {
            for (int z = 0; z <= dim; z++)
            {
                float fx = ((float)x / dim) * size - half;
                float fz = ((float)z / dim) * size - half;
                v[Index(x, z)] = new Vector3(fx, 0f, fz);
            }
        }
        return v;
    }

    int[] GenerateTriangles()
    {
        var tris = new int[dim * dim * 6];
        int t = 0;

        for (int x = 0; x < dim; x++)
        {
            for (int z = 0; z < dim; z++)
            {
                int i00 = Index(x, z);
                int i10 = Index(x + 1, z);
                int i01 = Index(x, z + 1);
                int i11 = Index(x + 1, z + 1);

                tris[t++] = i00; tris[t++] = i11; tris[t++] = i10;
                tris[t++] = i00; tris[t++] = i01; tris[t++] = i11;
            }
        }
        return tris;
    }

    Vector2[] GenerateUVs()
    {
        var u = new Vector2[dim1 * dim1];
        for (int x = 0; x <= dim; x++)
        {
            for (int z = 0; z <= dim; z++)
            {
                u[Index(x, z)] = new Vector2((float)x / dim, (float)z / dim);
            }
        }
        return u;
    }

    int Index(int x, int z) => x * dim1 + z;

    [Serializable]
    public struct Octave
    {
        public Vector2 speed;
        public Vector2 scale;
        public float height;
        public bool alternate;
    }
}
