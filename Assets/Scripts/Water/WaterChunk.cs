using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class WaterChunk : MonoBehaviour
{
    [Header("Tile")]
    public float tileSize = 300f;
    public int resolution = 64;

    Mesh mesh;
    Vector3[] verts;
    int dim, dim1;

    void Awake()
    {
        BuildMesh();
    }

    private void BuildMesh()
    {
        dim = Mathf.Max(2, resolution);
        dim1 = dim + 1;

        mesh = new Mesh();
        mesh.name = "EaterTileMesh";
        mesh.MarkDynamic();

        verts= new Vector3[dim1 * dim1];
        var uvs = new Vector2[dim1 * dim1];
        var tris = new int [dim * dim * 6];

        float half = tileSize * 0.5f;

        for (int x = 0; x <= dim; x++)
        {
            for (int z = 0; z <= dim; z++)
            {
                float fx = ((float)x / dim) * tileSize - half;
                float fz = ((float)z / dim) * tileSize - half;
                int i = Index(x, z);

                verts[i] = new Vector3(fx, 0f, fz);
                uvs[i] = new Vector2((float)x / dim, (float)z / dim);
            }
        }

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

        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    private void Update()
    {
        var ws = WaveSystem.Instance;
        if (!ws || mesh == null) return;

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 local = verts[i];
            Vector3 world = transform.TransformPoint(new Vector3(local.x, 0f, local.z));
            float h = ws.GetHeight(world);

            local.y = h - transform.position.y;
            verts[i] = local;
        }

        mesh.SetVertices(verts);
        mesh.RecalculateNormals();
    }

    int Index(int x, int z) => x * dim1 + z;
}
