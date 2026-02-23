using UnityEngine;
using UnityEditor;

public class TerrainHeightClampWindow : EditorWindow
{
    // Clamp target
    private Terrain targetTerrain;

    // Clamp values
    private float minWorldY = 1f;
    private float maxWorldY = 1f;
    private bool useSelectedTerrain = true;

    // Beach Ramp (Option B)
    private Terrain islandTerrain;
    private Terrain seafloorTerrain;
    private float waterlineY = 1f;

    [Tooltip("How far outward from the coastline the seafloor is forced to be >= waterline (meters).")]
    private float beachWidth = 12f;

    [Tooltip("How far outward it smoothly falls from waterline down to the current seafloor height (meters).")]
    private float falloff = 30f;

    [Tooltip("How strong the ramp is applied per run (0..1). 1 = overwrite (max), 0.5 = blend.")]
    private float strength = 1f;

    [MenuItem("Tools/Terrain/Clamp & Coast Tools")]
    public static void Open()
    {
        GetWindow<TerrainHeightClampWindow>("Clamp & Coast");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Terrain Clamp (Worldspace)", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        useSelectedTerrain = EditorGUILayout.ToggleLeft("Auto-use selected Terrain", useSelectedTerrain);
        if (useSelectedTerrain)
        {
            var sel = Selection.activeGameObject;
            if (sel != null)
            {
                var t = sel.GetComponent<Terrain>();
                if (t != null) targetTerrain = t;
            }
        }

        targetTerrain = (Terrain)EditorGUILayout.ObjectField("Target Terrain", targetTerrain, typeof(Terrain), true);
        minWorldY = EditorGUILayout.FloatField("Min World Y", minWorldY);
        maxWorldY = EditorGUILayout.FloatField("Max World Y", maxWorldY);

        using (new EditorGUI.DisabledScope(targetTerrain == null || targetTerrain.terrainData == null))
        {
            if (GUILayout.Button("Clamp MIN (raise low parts up)"))
                ClampMin(targetTerrain, minWorldY);

            if (GUILayout.Button("Clamp MAX (cut high parts down)"))
                ClampMax(targetTerrain, maxWorldY);

            if (GUILayout.Button("Clamp BOTH (min..max)"))
                ClampBoth(targetTerrain, minWorldY, maxWorldY);
        }

        EditorGUILayout.Space(14);
        EditorGUILayout.LabelField("Coast Tool (Option B: Beach Ramp)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Creates a smooth beach ramp on the SEAFLOOR around the ISLAND coastline.\n\n" +
            "Island must have Holes painted (water area = holes). The tool uses the hole edge as coastline.\n" +
            "It will set seafloor height near coast >= WaterlineY and smoothly fall off outward.",
            MessageType.Info);

        islandTerrain = (Terrain)EditorGUILayout.ObjectField("Island Terrain (holes)", islandTerrain, typeof(Terrain), true);
        seafloorTerrain = (Terrain)EditorGUILayout.ObjectField("Seafloor Terrain", seafloorTerrain, typeof(Terrain), true);

        waterlineY = EditorGUILayout.FloatField("Waterline World Y", waterlineY);
        beachWidth = EditorGUILayout.FloatField("Beach Width (m)", beachWidth);
        falloff = EditorGUILayout.FloatField("Falloff (m)", falloff);
        strength = EditorGUILayout.Slider("Strength", strength, 0f, 1f);

        using (new EditorGUI.DisabledScope(islandTerrain == null || seafloorTerrain == null))
        {
            if (GUILayout.Button("Apply Beach Ramp to Seafloor"))
            {
                ApplyBeachRamp(islandTerrain, seafloorTerrain, waterlineY, beachWidth, falloff, strength);
            }
        }
    }

    // ---------------- Clamp functions ----------------

    private static void ClampMax(Terrain terrain, float maxWorldY)
    {
        if (terrain == null || terrain.terrainData == null) return;

        var td = terrain.terrainData;
        float baseY = terrain.transform.position.y;

        float maxRelative = maxWorldY - baseY;
        if (maxRelative <= 0f)
        {
            Debug.LogWarning($"[TerrainClamp] MaxWorldY ({maxWorldY}) <= Terrain baseY ({baseY}). Flattening to base.");
            SetAllHeights(td, 0f);
            return;
        }

        float maxN = Mathf.Clamp01(maxRelative / td.size.y);

        Undo.RegisterCompleteObjectUndo(td, "Clamp Terrain MAX");

        int res = td.heightmapResolution;
        var heights = td.GetHeights(0, 0, res, res);

        bool changed = false;
        for (int y = 0; y < res; y++)
        for (int x = 0; x < res; x++)
        {
            float v = heights[y, x];
            if (v > maxN)
            {
                heights[y, x] = maxN;
                changed = true;
            }
        }

        if (changed)
        {
            td.SetHeights(0, 0, heights);
            EditorUtility.SetDirty(td);
        }

        Debug.Log($"[TerrainClamp] MAX done. MaxWorldY={maxWorldY} baseY={baseY} maxN={maxN:0.###} changed={changed}");
    }

    private static void ClampMin(Terrain terrain, float minWorldY)
    {
        if (terrain == null || terrain.terrainData == null) return;

        var td = terrain.terrainData;
        float baseY = terrain.transform.position.y;

        float minRelative = minWorldY - baseY;
        if (minRelative <= 0f)
        {
            Debug.Log($"[TerrainClamp] MIN: MinWorldY ({minWorldY}) <= baseY ({baseY}). Nothing to raise.");
            return;
        }

        float minN = Mathf.Clamp01(minRelative / td.size.y);

        Undo.RegisterCompleteObjectUndo(td, "Clamp Terrain MIN");

        int res = td.heightmapResolution;
        var heights = td.GetHeights(0, 0, res, res);

        bool changed = false;
        for (int y = 0; y < res; y++)
        for (int x = 0; x < res; x++)
        {
            float v = heights[y, x];
            if (v < minN)
            {
                heights[y, x] = minN;
                changed = true;
            }
        }

        if (changed)
        {
            td.SetHeights(0, 0, heights);
            EditorUtility.SetDirty(td);
        }

        Debug.Log($"[TerrainClamp] MIN done. MinWorldY={minWorldY} baseY={baseY} minN={minN:0.###} changed={changed}");
    }

    private static void ClampBoth(Terrain terrain, float minWorldY, float maxWorldY)
    {
        if (maxWorldY < minWorldY)
            (minWorldY, maxWorldY) = (maxWorldY, minWorldY);

        if (terrain == null || terrain.terrainData == null) return;

        var td = terrain.terrainData;
        float baseY = terrain.transform.position.y;

        float minRelative = minWorldY - baseY;
        float maxRelative = maxWorldY - baseY;

        float minN = Mathf.Clamp01(minRelative / td.size.y);
        float maxN = Mathf.Clamp01(maxRelative / td.size.y);

        Undo.RegisterCompleteObjectUndo(td, "Clamp Terrain BOTH");

        int res = td.heightmapResolution;
        var heights = td.GetHeights(0, 0, res, res);

        bool changed = false;
        for (int y = 0; y < res; y++)
        for (int x = 0; x < res; x++)
        {
            float v = heights[y, x];
            float c = Mathf.Clamp(v, minN, maxN);
            if (!Mathf.Approximately(c, v))
            {
                heights[y, x] = c;
                changed = true;
            }
        }

        if (changed)
        {
            td.SetHeights(0, 0, heights);
            EditorUtility.SetDirty(td);
        }

        Debug.Log($"[TerrainClamp] BOTH done. MinWorldY={minWorldY} MaxWorldY={maxWorldY} baseY={baseY} changed={changed}");
    }

    private static void SetAllHeights(TerrainData td, float normalizedHeight)
    {
        int res = td.heightmapResolution;
        float[,] heights = new float[res, res];
        for (int y = 0; y < res; y++)
        for (int x = 0; x < res; x++)
            heights[y, x] = normalizedHeight;

        td.SetHeights(0, 0, heights);
        EditorUtility.SetDirty(td);
    }

    // ---------------- Coast tool (Option B) ----------------

    private static void ApplyBeachRamp(
        Terrain island, Terrain seafloor,
        float waterY, float beachWidthMeters, float falloffMeters,
        float strength01)
    {
        if (island == null || seafloor == null) return;
        if (island.terrainData == null || seafloor.terrainData == null) return;

        var islandTD = island.terrainData;
        var seaTD = seafloor.terrainData;

        // Holes data from island
        int holesRes = islandTD.holesResolution;
        bool[,] holes = islandTD.GetHoles(0, 0, holesRes, holesRes);

        // Sea heightmap
        int seaRes = seaTD.heightmapResolution;
        float[,] seaHeights = seaTD.GetHeights(0, 0, seaRes, seaRes);

        Vector3 seaPos = seafloor.transform.position;
        Vector3 seaSize = seaTD.size;

        Vector3 islandPos = island.transform.position;
        Vector3 islandSize = islandTD.size;

        // Convert meters to a small search step (trade quality vs speed)
        float maxSearch = Mathf.Max(1f, beachWidthMeters + falloffMeters);
        float step = Mathf.Clamp(maxSearch / 16f, 0.5f, 4f); // meters per sample

        Undo.RegisterCompleteObjectUndo(seaTD, "Apply Beach Ramp");

        int changed = 0;

        for (int y = 0; y < seaRes; y++)
        {
            for (int x = 0; x < seaRes; x++)
            {
                // Sea sample world position
                float u = x / (float)(seaRes - 1);
                float v = y / (float)(seaRes - 1);

                float wx = seaPos.x + u * seaSize.x;
                float wz = seaPos.z + v * seaSize.z;

                // Only where seafloor overlaps island XZ (saves work)
                if (wx < islandPos.x || wx > islandPos.x + islandSize.x ||
                    wz < islandPos.z || wz > islandPos.z + islandSize.z)
                    continue;

                // Determine if this point lies in "water area" of island (hole = water)
                if (!TryWorldToHoleIndex(islandPos, islandSize, holesRes, wx, wz, out int hx, out int hy))
                    continue;

                bool isWaterHole = !holes[hy, hx];
                if (!isWaterHole)
                    continue; // Only shape seafloor in water near coast

                // Distance from this water point to nearest land (hole=false) on island terrain
                float distToLand = DistanceToNearestLand(holes, holesRes, islandPos, islandSize, wx, wz, maxSearch, step);
                if (distToLand < 0f)
                    continue;

                // Build desired world Y for seafloor
                //  - 0..beachWidth -> waterline
                //  - beachWidth..beachWidth+falloff -> smoothly blend down to current seafloor height
                float currentWorldY = seaPos.y + seaHeights[y, x] * seaSize.y;

                float desiredWorldY;
                if (distToLand <= beachWidthMeters)
                {
                    desiredWorldY = waterY;
                }
                else
                {
                    float t = Mathf.InverseLerp(beachWidthMeters, beachWidthMeters + Mathf.Max(0.001f, falloffMeters), distToLand);
                    // Smooth curve (nice beach)
                    t = Smooth01(t);
                    desiredWorldY = Mathf.Lerp(waterY, currentWorldY, t);
                }

                // We only lift seafloor up (avoid making holes/valleys by mistake)
                if (desiredWorldY <= currentWorldY)
                    continue;

                float desiredN = Mathf.Clamp01((desiredWorldY - seaPos.y) / seaSize.y);
                float blended = Mathf.Lerp(seaHeights[y, x], desiredN, Mathf.Clamp01(strength01));

                if (!Mathf.Approximately(blended, seaHeights[y, x]))
                {
                    seaHeights[y, x] = blended;
                    changed++;
                }
            }
        }

        seaTD.SetHeights(0, 0, seaHeights);
        EditorUtility.SetDirty(seaTD);

        Debug.Log($"[BeachRamp] Done. Changed samples: {changed}. " +
                  $"WaterlineY={waterY}, BeachWidth={beachWidthMeters}m, Falloff={falloffMeters}m, Strength={strength01}");
    }

    private static float Smooth01(float t)
    {
        // smoothstep
        t = Mathf.Clamp01(t);
        return t * t * (3f - 2f * t);
    }

    private static bool TryWorldToHoleIndex(Vector3 terrainPos, Vector3 terrainSize, int holesRes, float wx, float wz, out int ix, out int iy)
    {
        float u = Mathf.InverseLerp(terrainPos.x, terrainPos.x + terrainSize.x, wx);
        float v = Mathf.InverseLerp(terrainPos.z, terrainPos.z + terrainSize.z, wz);
        if (u < 0f || u > 1f || v < 0f || v > 1f)
        {
            ix = iy = 0;
            return false;
        }

        ix = Mathf.Clamp(Mathf.RoundToInt(u * (holesRes - 1)), 0, holesRes - 1);
        iy = Mathf.Clamp(Mathf.RoundToInt(v * (holesRes - 1)), 0, holesRes - 1);
        return true;
    }

    // Returns distance in meters from a world point (in water hole) to nearest land cell (hole=false).
    private static float DistanceToNearestLand(bool[,] holes, int holesRes, Vector3 pos, Vector3 size, float wx, float wz, float maxRadius, float stepMeters)
    {
        // If we're already on land (shouldn't happen because caller checks), return 0
        if (!TryWorldToHoleIndex(pos, size, holesRes, wx, wz, out int cx, out int cy))
            return -1f;
        if (holes[cy, cx]) return 0f;

        float best = float.MaxValue;

        // Scan a square in worldspace around point
        for (float dz = -maxRadius; dz <= maxRadius; dz += stepMeters)
        {
            for (float dx = -maxRadius; dx <= maxRadius; dx += stepMeters)
            {
                float nx = wx + dx;
                float nz = wz + dz;

                if (!TryWorldToHoleIndex(pos, size, holesRes, nx, nz, out int ix, out int iy))
                    continue;

                if (holes[iy, ix]) // land found
                {
                    float d = Mathf.Sqrt(dx * dx + dz * dz);
                    if (d < best) best = d;
                }
            }
        }

        return (best == float.MaxValue) ? -1f : best;
    }
}