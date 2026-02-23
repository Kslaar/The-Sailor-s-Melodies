using UnityEngine;
using UnityEditor;

public class IslandTerrainBuilder : EditorWindow
{
    [Header("Inputs")]
    public Terrain targetTerrain;      // das EINE Terrain (z.B. Terrain_Seafloor)
    public Terrain maskTerrain;        // dein Final_Island Terrain (mit Holes)

    [Header("Heights (World Y)")]
    public float waterlineY = 0f;
    public float plateauY = 4f;
    public float grassStartY = 1.5f;

    [Header("Coast shaping (meters)")]
    public float coastWidth = 18f;     // wie breit ist der Steilhang
    public float coastSmooth = 1.0f;   // 0 = harte Kante, 1 = smoothstep

    [Header("Texture Paint")]
    public int sandLayerIndex = 0;
    public int grassLayerIndex = 1;
    public bool hardCut = true;        // keine Mischung

    [MenuItem("Tools/Terrain/Island Builder (Holes -> Heights + AutoTexture)")]
    public static void Open()
    {
        GetWindow<IslandTerrainBuilder>("Island Builder");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Island Builder", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        targetTerrain = (Terrain)EditorGUILayout.ObjectField("Target Terrain (ONE)", targetTerrain, typeof(Terrain), true);
        maskTerrain   = (Terrain)EditorGUILayout.ObjectField("Mask Terrain (with Holes)", maskTerrain, typeof(Terrain), true);

        EditorGUILayout.Space(8);
        waterlineY = EditorGUILayout.FloatField("Waterline Y", waterlineY);
        plateauY   = EditorGUILayout.FloatField("Plateau Y", plateauY);
        grassStartY = EditorGUILayout.FloatField("Grass starts above Y", grassStartY);

        EditorGUILayout.Space(8);
        coastWidth  = EditorGUILayout.FloatField("Coast width (m)", coastWidth);
        coastSmooth = EditorGUILayout.Slider("Coast smooth", coastSmooth, 0f, 1f);

        EditorGUILayout.Space(8);
        sandLayerIndex  = EditorGUILayout.IntField("Sand Layer Index", sandLayerIndex);
        grassLayerIndex = EditorGUILayout.IntField("Grass Layer Index", grassLayerIndex);
        hardCut         = EditorGUILayout.ToggleLeft("Hard cut (no blend)", hardCut);

        EditorGUILayout.Space(10);

        using (new EditorGUI.DisabledScope(targetTerrain == null || maskTerrain == null))
        {
            if (GUILayout.Button("1) Bake Island Heights from Holes"))
                BakeHeightsFromHoles();

            if (GUILayout.Button("2) Auto-Paint Sand/Grass by Height"))
                AutoPaintByHeight();
        }

        EditorGUILayout.HelpBox(
            "Workflow:\n" +
            "1) Use your existing Holes on Mask Terrain to define land vs water.\n" +
            "2) Bake heights onto Target Terrain: land becomes plateau, coast slopes down to waterline.\n" +
            "3) Auto-paint textures by world height: sand <= grassStartY, grass > grassStartY.",
            MessageType.Info);
    }

    // ---------- 1) Heights ----------
    private void BakeHeightsFromHoles()
    {
        var t = targetTerrain;
        var m = maskTerrain;

        var td = t.terrainData;
        var md = m.terrainData;

        // mask holes: true = surface, false = hole
        int holesRes = md.holesResolution;
        bool[,] holes = md.GetHoles(0, 0, holesRes, holesRes);

        int res = td.heightmapResolution;
        float[,] heights = td.GetHeights(0, 0, res, res);

        Vector3 tPos = t.transform.position;
        Vector3 tSize = td.size;

        Vector3 mPos = m.transform.position;
        Vector3 mSize = md.size;

        float maxSearch = Mathf.Max(1f, coastWidth);
        float step = Mathf.Clamp(maxSearch / 18f, 0.5f, 3.0f);

        Undo.RegisterCompleteObjectUndo(td, "Bake Island Heights from Holes");

        int changed = 0;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float u = x / (float)(res - 1);
                float v = y / (float)(res - 1);

                float wx = tPos.x + u * tSize.x;
                float wz = tPos.z + v * tSize.z;

                // if outside mask bounds -> treat as water/no island influence
                if (wx < mPos.x || wx > mPos.x + mSize.x || wz < mPos.z || wz > mPos.z + mSize.z)
                    continue;

                if (!TryWorldToHoleIndex(mPos, mSize, holesRes, wx, wz, out int hx, out int hy))
                    continue;

                bool isLand = holes[hy, hx]; // true = surface = land, false = hole = water

                float desiredWorldY;

                if (isLand)
                {
                    // On land: plateau
                    desiredWorldY = plateauY;
                }
                else
                {
                    // In water region: only shape near coast
                    float distToLand = DistanceToNearestLand(holes, holesRes, mPos, mSize, wx, wz, maxSearch, step);
                    if (distToLand < 0f) continue; // far from land -> leave as is

                    // coast profile: at coast (0m) = plateauY, at coastWidth = waterlineY
                    float t01 = Mathf.Clamp01(distToLand / Mathf.Max(0.001f, coastWidth));
                    float s = Smooth(t01, coastSmooth);

                    desiredWorldY = Mathf.Lerp(plateauY, waterlineY, s);
                }

                float currentWorldY = tPos.y + heights[y, x] * tSize.y;

                // we only raise to desired if it would be higher; BUT for coast we also want to pull down steep bits
                // so we set directly
                float desiredN = Mathf.Clamp01((desiredWorldY - tPos.y) / tSize.y);

                if (!Mathf.Approximately(heights[y, x], desiredN))
                {
                    heights[y, x] = desiredN;
                    changed++;
                }
            }
        }

        td.SetHeights(0, 0, heights);
        EditorUtility.SetDirty(td);

        Debug.Log($"[IslandBuilder] BakeHeights done. Changed samples: {changed}");
    }

    private static float Smooth(float t, float smoothness)
    {
        t = Mathf.Clamp01(t);
        float smoothstep = t * t * (3f - 2f * t);
        return Mathf.Lerp(t, smoothstep, smoothness);
    }

    private static bool TryWorldToHoleIndex(Vector3 pos, Vector3 size, int holesRes, float wx, float wz, out int ix, out int iy)
    {
        float u = Mathf.InverseLerp(pos.x, pos.x + size.x, wx);
        float v = Mathf.InverseLerp(pos.z, pos.z + size.z, wz);
        if (u < 0f || u > 1f || v < 0f || v > 1f)
        {
            ix = iy = 0;
            return false;
        }
        ix = Mathf.Clamp(Mathf.RoundToInt(u * (holesRes - 1)), 0, holesRes - 1);
        iy = Mathf.Clamp(Mathf.RoundToInt(v * (holesRes - 1)), 0, holesRes - 1);
        return true;
    }

    // from a water point, find nearest land (holes==true)
    private static float DistanceToNearestLand(bool[,] holes, int holesRes, Vector3 pos, Vector3 size, float wx, float wz, float maxRadius, float stepMeters)
    {
        if (!TryWorldToHoleIndex(pos, size, holesRes, wx, wz, out int cx, out int cy))
            return -1f;

        if (holes[cy, cx]) return 0f; // already land

        float best = float.MaxValue;

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

    // ---------- 2) Texture paint ----------
    private void AutoPaintByHeight()
    {
        var t = targetTerrain;
        var td = t.terrainData;

        int layers = td.terrainLayers != null ? td.terrainLayers.Length : 0;
        if (layers < 2)
        {
            Debug.LogError("[IslandBuilder] Target Terrain needs at least 2 TerrainLayers (Sand index 0, Grass index 1).");
            return;
        }

        if (sandLayerIndex < 0 || sandLayerIndex >= layers || grassLayerIndex < 0 || grassLayerIndex >= layers)
        {
            Debug.LogError("[IslandBuilder] Layer indices out of range.");
            return;
        }

        int aW = td.alphamapWidth;
        int aH = td.alphamapHeight;
        int aL = td.alphamapLayers;

        float[,,] alpha = td.GetAlphamaps(0, 0, aW, aH);

        Vector3 pos = t.transform.position;
        Vector3 size = td.size;

        Undo.RegisterCompleteObjectUndo(td, "Auto Paint Terrain Layers by Height");

        for (int y = 0; y < aH; y++)
        {
            for (int x = 0; x < aW; x++)
            {
                float u = x / (float)(aW - 1);
                float v = y / (float)(aH - 1);

                float wx = pos.x + u * size.x;
                float wz = pos.z + v * size.z;

                float worldY = t.SampleHeight(new Vector3(wx, 0f, wz)) + pos.y;

                bool grass = worldY > grassStartY;

                // reset
                for (int l = 0; l < aL; l++) alpha[y, x, l] = 0f;

                if (hardCut)
                {
                    alpha[y, x, grass ? grassLayerIndex : sandLayerIndex] = 1f;
                }
                else
                {
                    // tiny blend band (optional)
                    float blendBand = 0.25f;
                    float t01 = Mathf.InverseLerp(grassStartY - blendBand, grassStartY + blendBand, worldY);
                    alpha[y, x, sandLayerIndex] = 1f - t01;
                    alpha[y, x, grassLayerIndex] = t01;
                }
            }
        }

        td.SetAlphamaps(0, 0, alpha);
        EditorUtility.SetDirty(td);

        Debug.Log("[IslandBuilder] AutoPaintByHeight done.");
    }
}