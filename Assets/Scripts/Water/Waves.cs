using System;
using UnityEngine;

[DisallowMultipleComponent]
public class Waves : MonoBehaviour
{
    public int dimension = 10;
    public Octave[] octaves;
    public float uvScale;

    MeshFilter meshFilter;
    Mesh mesh;

    // Cache
    Vector3[] vertices;
    int dim1;                  
    Vector3 invLossyXZ;        

    void Start()
    {
        dim1 = dimension + 1;

        mesh = new Mesh();
        mesh.name = gameObject.name;

        vertices = GenerateVertices();                
        mesh.vertices = vertices;
        mesh.triangles = GenerateTriangles(vertices.Length);
        mesh.uv = GenerateUVs(vertices.Length);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;

        CacheScale();
    }

    void CacheScale()
    {
        var s = transform.lossyScale;
        invLossyXZ = new Vector3(
            s.x != 0f ? 1f / s.x : 0f,
            0f,
            s.z != 0f ? 1f / s.z : 0f
        );
    }

    public float GetHeightFromPoint(Vector3 position)
    {
        var localPos = Vector3.Scale(position - transform.position, invLossyXZ);

        float lx = Mathf.Clamp(localPos.x, 0f, dimension);
        float lz = Mathf.Clamp(localPos.z, 0f, dimension);

        int x0 = Mathf.FloorToInt(lx);
        int z0 = Mathf.FloorToInt(lz);
        int x1 = Mathf.Min(x0 + 1, dimension);
        int z1 = Mathf.Min(z0 + 1, dimension);

        // Bilinear Interpolation (viel günstiger!!!)
        float tx = lx - x0;
        float tz = lz - z0;

        float h00 = vertices[Index(x0, z0)].y;
        float h01 = vertices[Index(x0, z1)].y;
        float h10 = vertices[Index(x1, z0)].y;
        float h11 = vertices[Index(x1, z1)].y;

        float h0 = Mathf.Lerp(h00, h10, tx);
        float h1 = Mathf.Lerp(h01, h11, tx);
        float h = Mathf.Lerp(h0, h1, tz);

        return h * transform.lossyScale.y;
    }

    Vector3[] GenerateVertices()
    {
        var v = new Vector3[dim1 * dim1];
        for (int x = 0; x <= dimension; x++)
            for (int z = 0; z <= dimension; z++)
                v[Index(x, z)] = new Vector3(x, 0, z);
        return v;
    }

    int Index(int x, int z) => x * dim1 + z;

    int[] GenerateTriangles(int vertCount)
    {
        var triangles = new int[vertCount * 6];
        for (int x = 0; x < dimension; x++)
        {
            for (int z = 0; z < dimension; z++)
            {
                int t = Index(x, z) * 6;
                triangles[t + 0] = Index(x, z);
                triangles[t + 1] = Index(x + 1, z + 1);
                triangles[t + 2] = Index(x + 1, z);
                triangles[t + 3] = Index(x, z);
                triangles[t + 4] = Index(x, z + 1);
                triangles[t + 5] = Index(x + 1, z + 1);
            }
        }
        return triangles;
    }

    Vector2[] GenerateUVs(int vertCount)
    {
        var uvs = new Vector2[vertCount];
        for (int x = 0; x <= dimension; x++)
            for (int z = 0; z <= dimension; z++)
                uvs[Index(x, z)] = new Vector2((float)x / dimension, (float)z / dimension);
        return uvs;
    }

    void Update()
    {
        for (int x = 0; x <= dimension; x++)
        {
            for (int z = 0; z <= dimension; z++)
            {
                float y = 0f;
                for (int o = 0; o < octaves.Length; o++)
                {
                    if (octaves[o].alternate)
                    {
                        float perlin = Mathf.PerlinNoise((x * octaves[o].scale.x) / dimension,
                                                        (z * octaves[o].scale.y) / dimension) * Mathf.PI * 2f;
                        y += Mathf.Cos(perlin + octaves[o].speed.magnitude * Time.time) * octaves[o].height;
                    }
                    else
                    {
                        float perlin = Mathf.PerlinNoise((x * octaves[o].scale.x + Time.time * octaves[o].speed.x) / dimension,
                                                        (z * octaves[o].scale.y + Time.time * octaves[o].speed.y) / dimension) - 0.5f;
                        y += perlin * octaves[o].height;
                    }
                }

                int idx = Index(x, z);
                var p = vertices[idx];
                p.y = y;
                vertices[idx] = p;
            }
        }

        mesh.SetVertices(vertices);         
        mesh.RecalculateNormals();
    }

    [Serializable]
    public struct Octave
    {
        public Vector2 speed;
        public Vector2 scale;
        public float height;
        public bool alternate;
    }
}
