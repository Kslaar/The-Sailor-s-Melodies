using System;
using UnityEngine;

public class Waves : MonoBehaviour
{
    public int dimension = 10;
    public Octave[] octaves;

    protected MeshFilter meshFilter;
    protected Mesh mesh;
    void Start()
    {
        mesh = new Mesh();
        mesh.name = gameObject.name;

        mesh.vertices = GenerateVertices();
        mesh.triangles = GenerateTriangles();
        // mesh.uv = GenerateUVs();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }


    private Vector3[] GenerateVertices()
    {
        var vertices = new Vector3[(dimension + 1) * (dimension + 1)];

        // vertices gleichmäßig verteilen
        for (int x = 0; x <= dimension; x++)
        {
            for (int z = 0; z <= dimension; z++)
            {
                vertices[Index(x, z)] = new Vector3(x, 0, z);
            }
        }

        return vertices;
    }

    private int Index(int x, int z)
    {
        return x * (dimension + 1) + z;
    }

    private int[] GenerateTriangles()
    {
        var triangles = new int[mesh.vertices.Length * 6];

        for (int x = 0; x < dimension; x++)
        {
            for (int z = 0; z < dimension; z++)
            {
                triangles[Index(x, z) * 6 + 0] = Index(x, z);
                triangles[Index(x, z) * 6 + 1] = Index(x + 1, z + 1);
                triangles[Index(x, z) * 6 + 2] = Index(x + 1, z);
                triangles[Index(x, z) * 6 + 3] = Index(x, z);
                triangles[Index(x, z) * 6 + 4] = Index(x, z + 1);
                triangles[Index(x, z) * 6 + 5] = Index(x + 1, z + 1);
            }
        }

        return triangles;
    }

    // private Vector2[] GenerateUVs()
    

    // Update is called once per frame
    void Update()
    {
        var vertices = mesh.vertices;
        for (int x = 0; x < dimension; x++)
        {
            for (int z = 0; z < dimension; z++)
            {
                var y = 0f;
                for (int o = 0; o < octaves.Length; o++)
                {
                    if (octaves[o].alternate)
                    {
                        var perlin = Mathf.PerlinNoise((x * octaves[o].scale.x) / dimension, (z * octaves[o].scale.y) / dimension) * Mathf.PI * 2f;
                        y += Mathf.Cos(octaves[o].speed.magnitude * Time.time) * octaves[o].height;
                    }
                    else
                    {
                        var perlin = Mathf.PerlinNoise((x * octaves[o].scale.x + Time.time * octaves[o].speed.x) / dimension, (z * octaves[o].scale.y + Time.time * octaves[o].speed.y) / dimension) - 0.5f;
                        y += perlin * octaves[o].height;
                    }
                }
                vertices[Index(x, z)] = new Vector3(x, y, z);
            }
        }
        mesh.vertices = vertices;
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
