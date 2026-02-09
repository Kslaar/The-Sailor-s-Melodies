using UnityEngine;

public class WaterChunkManager : MonoBehaviour
{
    public Transform target;
    public WaterChunk tilePrefab;

    [Header("Grid")]
    public int tilesPerAxis = 3;
    public float tileSize = 300f;
    public int tileResolution = 64;

    WaterChunk[,] tiles;
    int half;

    void Start()
    {
        half = tilesPerAxis / 2;
        tiles = new WaterChunk[tilesPerAxis, tilesPerAxis];

        for (int x = 0; x < tilesPerAxis; x++)
        {
            for (int z = 0; z < tilesPerAxis; z++)
            {
                var t = Instantiate(tilePrefab, transform);
                t.tileSize = tileSize;
                t.resolution = tileResolution;

                tiles[x, z] = t;
            }
        }

        UpdateTiles(true);
    }

    void Update()
    {
        UpdateTiles(false);
    }

    private void UpdateTiles(bool force)
    {
        if (!target) return;

        Vector3 p = target.position;

        int chunkX = Mathf.FloorToInt(p.x / tileSize);
        int chunkZ = Mathf.FloorToInt(p.z / tileSize);

        for (int x = 0; x < tilesPerAxis; x++)
        {
            for (int z = 0; z < tilesPerAxis; z++)
            {
                int gridX = chunkX + (x - half);
                int gridZ = chunkZ + (z - half);

                Vector3 center = new Vector3((gridX + 0.5f) * tileSize, tiles[x, z].transform.position.y, (gridZ + 0.5f) * tileSize);

                if (force || (tiles[x, z].transform.position - center).sqrMagnitude > 0.01f)
                    tiles[x, z].transform.position = center; 
            }
        }
    }
}
