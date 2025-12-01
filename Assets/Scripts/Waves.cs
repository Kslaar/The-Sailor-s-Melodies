using System;
using UnityEngine;
using UnityEngine.SocialPlatforms;

public class Waves : MonoBehaviour
{
    public int dimension = 10;
    public Octave[] octaves;
    public float uvScale;

    protected MeshFilter meshFilter;
    protected Mesh mesh;
    void Start()
    {
        mesh = new Mesh();
        mesh.name = gameObject.name;

        mesh.vertices = GenerateVertices();
        mesh.triangles = GenerateTriangles();
        mesh.uv = GenerateUVs();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    public float GetHeightFromPoint(Vector3 position)
    {
        var scale = new Vector3(1 / transform.lossyScale.x, 0, 1 / transform.lossyScale.z);
        var localPos = Vector3.Scale((position - transform.position), scale);

        var pos1 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Floor(localPos.z));
        var pos2 = new Vector3(Mathf.Floor(localPos.x), 0, Mathf.Ceil(localPos.z));
        var pos3 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Floor(localPos.z));
        var pos4 = new Vector3(Mathf.Ceil(localPos.x), 0, Mathf.Ceil(localPos.z));

        pos1.x = Mathf.Clamp(pos1.x, 0, dimension);
        pos1.z = Mathf.Clamp(pos1.z, 0, dimension);
        pos2.x = Mathf.Clamp(pos2.x, 0, dimension);
        pos2.z = Mathf.Clamp(pos2.z, 0, dimension);
        pos3.x = Mathf.Clamp(pos3.x, 0, dimension);
        pos3.z = Mathf.Clamp(pos3.z, 0, dimension);
        pos4.x = Mathf.Clamp(pos4.x, 0, dimension);
        pos4.z = Mathf.Clamp(pos4.z, 0, dimension);

        var max = Mathf.Max(Vector3.Distance(pos1, localPos), Vector3.Distance(pos2, localPos), Vector3.Distance(pos3, localPos), Vector3.Distance(pos4, localPos) + Mathf.Epsilon);
        var distance = (max - Vector3.Distance(pos1, localPos)) +
                       (max - Vector3.Distance(pos2, localPos)) +
                       (max - Vector3.Distance(pos3, localPos)) +
                       (max - Vector3.Distance(pos4, localPos)) + Mathf.Epsilon;
                
        var height = mesh.vertices[Index((int)pos1.x, (int)pos1.z)].y * (max - Vector3.Distance(pos1, localPos))
                   + mesh.vertices[Index((int)pos2.x, (int)pos2.z)].y * (max - Vector3.Distance(pos2, localPos))
                   + mesh.vertices[Index((int)pos3.x, (int)pos3.z)].y * (max - Vector3.Distance(pos3, localPos))
                   + mesh.vertices[Index((int)pos4.x, (int)pos4.z)].y * (max - Vector3.Distance(pos4, localPos));

        return height * transform.lossyScale.y / distance;
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

    private Vector2[] GenerateUVs()
    {
        var uvs = new Vector2[mesh.vertices.Length];

        for (int x = 0; x <= dimension; x++)
        {
            for (int z = 0; z <= dimension; z++)
            {
                // var vctr = new Vector2((x / uvScale) % 2, (z / uvScale) % 2);
                uvs[Index(x, z)] = new Vector2((float)x / dimension, (float)z / dimension);
            }
        }

        return uvs;
    }
    

    // Update is called once per frame
    void Update()
    {
        var vertices = mesh.vertices;
        for (int x = 0; x <= dimension; x++)
        {
            for (int z = 0; z <= dimension; z++)
            {
                var y = 0f;
                for (int o = 0; o < octaves.Length; o++)
                {
                    if (octaves[o].alternate)
                    {
                        var perlin = Mathf.PerlinNoise((x * octaves[o].scale.x) / dimension, (z * octaves[o].scale.y) / dimension) * Mathf.PI * 2f;
                        y += Mathf.Cos(perlin + octaves[o].speed.magnitude * Time.time) * octaves[o].height;
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
