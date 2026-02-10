using System;
using UnityEngine;

public class WaveSystem : MonoBehaviour
{
    public static WaveSystem Instance { get; private set; }

    public float seaLevel = 0f;

    [Header("Wave Settings")]
    public Octave[] octaves;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public float GetHeight(Vector3 worldPos)
    {
        return SampleHeight(worldPos.x, worldPos.z, Time.time);
    }

    public Vector3 GetNormal(Vector3 worldPos, float gap = 0.5f)
    {
        float hLeft = SampleHeight(worldPos.x - gap, worldPos.z, Time.time);
        float hRight = SampleHeight(worldPos.x + gap, worldPos.z, Time.time);
        float hDown = SampleHeight(worldPos.x, worldPos.z - gap, Time.time);
        float hUp = SampleHeight(worldPos.x, worldPos.z + gap, Time.time);

        Vector3 n = new Vector3(hLeft - hRight, 2f * gap, hDown - hUp).normalized;
        return n;
    }

    float SampleHeight(float worldX, float worldZ, float time)
    {
        float y = 0f;

        for (int o = 0; o < octaves.Length; o++)
        {
            var oc = octaves[o];

            if (oc.alternate)
            {
                float n = Mathf.PerlinNoise(worldX * oc.scale.x, worldZ * oc.scale.y) * Mathf.PI * 2f;
                y += Mathf.Cos(n + oc.speed.magnitude * time) * oc.height;
            }
            else
            {
                float n = Mathf.PerlinNoise(worldX * oc.scale.x + time * oc.speed.x,
                                            worldZ * oc.scale.y + time * oc.speed.y) - 0.5f;
                y += n * oc.height;
            }
        }

        return seaLevel + y;
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
