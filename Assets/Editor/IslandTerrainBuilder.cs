using UnityEngine;
using UnityEditor;

public class IslandTerrainBuilder : EditorWindow
{
    [Header("Inputs")]
    public Terrain targetTerrain;
    public Terrain maskTerrain; // Holes: true=land/surface, false=water/hole

    [Header("Heights (World Y)")]
    public float waterlineY = 0f;
    public float plateauY = 7f;
    public float grassStartY = 1.5f;

    [Header("Cliff (steep coast)")]
    public float cliffWidth = 3f;
    [Range(0f, 1f)] public float cliffSmooth = 0f;

    [Header("Underwater Dropoff (no sandbanks)")]
    public float dropDepthY = -10f;
    public float dropoffDistance = 15f;
    [Range(0f, 1f)] public float dropoffSmooth = 0.15f;

    [Header("Mask Smoothing (smooth island outline / silhouette)")]
    [Tooltip("Iterations of smoothing on the mask (more = rounder outline). 2-5 is typical.")]
    public int maskSmoothIterations = 3;

    [Tooltip("Threshold for smoothed mask (0..1). 0.5 = majority. Higher -> shrink islands, lower -> expand.")]
    [Range(0f, 1f)] public float maskSmoothThreshold = 0.5f;

    [Header("Texture Paint")]
    public int sandLayerIndex = 0;
    public int grassLayerIndex = 1;
    public bool hardCut = true;

    [Header("Waterline texture rule (fix grass under water)")]
    public bool forceSandUnderWater = true;
    public float waterlineEpsilon = 0.05f;

    [Header("Cleanup / Safety")]
    public bool clearTargetHolesBeforeBake = true;
    public bool clearTargetHolesAfterBake = true;
    public bool forceNormalizeAlphamaps = true;
    public bool logLayerNames = true;

    [MenuItem("Tools/Terrain/Island Builder (Cliffs + Dropoff + AutoTexture)")]
    public static void Open()
    {
        GetWindow<IslandTerrainBuilder>("Island Builder");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Island Builder (Smooth Outline + Steep Cliffs + No Sandbanks)", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        targetTerrain = (Terrain)EditorGUILayout.ObjectField("Target Terrain (ONE)", targetTerrain, typeof(Terrain), true);
        maskTerrain = (Terrain)EditorGUILayout.ObjectField("Mask Terrain (with Holes)", maskTerrain, typeof(Terrain), true);

        EditorGUILayout.Space(8);
        waterlineY = EditorGUILayout.FloatField("Waterline Y", waterlineY);
        plateauY = EditorGUILayout.FloatField("Plateau Y", plateauY);
        grassStartY = EditorGUILayout.FloatField("Grass starts above Y", grassStartY);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Cliff", EditorStyles.boldLabel);
        cliffWidth = EditorGUILayout.FloatField("Cliff width (m)", cliffWidth);
        cliffSmooth = EditorGUILayout.Slider("Cliff smooth", cliffSmooth, 0f, 1f);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Underwater Dropoff", EditorStyles.boldLabel);
        dropDepthY = EditorGUILayout.FloatField("Drop depth Y", dropDepthY);
        dropoffDistance = EditorGUILayout.FloatField("Dropoff distance (m)", dropoffDistance);
        dropoffSmooth = EditorGUILayout.Slider("Dropoff smooth", dropoffSmooth, 0f, 1f);

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Mask Smoothing (Outline)", EditorStyles.boldLabel);
        maskSmoothIterations = EditorGUILayout.IntSlider("Smooth iterations", maskSmoothIterations, 0, 10);
        maskSmoothThreshold = EditorGUILayout.Slider("Smooth threshold", maskSmoothThreshold, 0f, 1f);

        EditorGUILayout.Space(8);
        sandLayerIndex = EditorGUILayout.IntField("Sand Layer Index", sandLayerIndex);
        grassLayerIndex = EditorGUILayout.IntField("Grass Layer Index", grassLayerIndex);
        hardCut = EditorGUILayout.ToggleLeft("Hard cut (no blend)", hardCut);

        EditorGUILayout.Space(8);
        forceSandUnderWater = EditorGUILayout.ToggleLeft("Force sand under waterline", forceSandUnderWater);
        waterlineEpsilon = EditorGUILayout.FloatField("Waterline epsilon", waterlineEpsilon);

        EditorGUILayout.Space(8);
        clearTargetHolesBeforeBake = EditorGUILayout.ToggleLeft("Clear Target Holes BEFORE Bake", clearTargetHolesBeforeBake);
        clearTargetHolesAfterBake = EditorGUILayout.ToggleLeft("Clear Target Holes AFTER Bake", clearTargetHolesAfterBake);
        forceNormalizeAlphamaps = EditorGUILayout.ToggleLeft("Normalize Alphamaps", forceNormalizeAlphamaps);
        logLayerNames = EditorGUILayout.ToggleLeft("Log TerrainLayer names", logLayerNames);

        EditorGUILayout.Space(10);

        using (new EditorGUI.DisabledScope(targetTerrain == null || targetTerrain.terrainData == null))
        {
            if (GUILayout.Button("0) CLEAR ALL HOLES on Target"))
                ClearAllHolesOnTarget();

            using (new EditorGUI.DisabledScope(maskTerrain == null || maskTerrain.terrainData == null))
            {
                if (GUILayout.Button("1) Bake Heights (Cliff + Dropoff + Smooth Outline)"))
                    BakeHeightsCliffDropoffSmoothOutline();
            }

            if (GUILayout.Button("2) Auto-Paint Sand/Grass by Height (No grass under water)"))
                AutoPaintByHeight();
        }

        EditorGUILayout.HelpBox(
            "What this does:\n" +
            "- Uses Mask Terrain Holes to define land vs water.\n" +
            "- Smooths the mask (rounder island silhouette) BEFORE baking heights.\n" +
            "- Bakes: Land becomes plateau, coast drops steeply to waterline, then underwater drops deeper.\n" +
            "- AutoPaint: No grass under waterline, grass only above grassStartY.\n",
            MessageType.Info);
    }

    // ---------------- Holes cleanup on Target ----------------
    private void ClearAllHolesOnTarget()
    {
        var td = targetTerrain.terrainData;
        int r = td.holesResolution;

        Undo.RegisterCompleteObjectUndo(td, "Clear All Terrain Holes");

        bool[,] allSolid = new bool[r, r];
        for (int y = 0; y < r; y++)
            for (int x = 0; x < r; x++)
                allSolid[y, x] = true;

        td.SetHoles(0, 0, allSolid);
        EditorUtility.SetDirty(td);
        Debug.Log($"[IslandBuilder] Cleared ALL holes on Target. holesRes={r}");
    }

    // ---------------- Bake Heights: Cliff + Dropoff + Smoothed Outline ----------------
    private void BakeHeightsCliffDropoffSmoothOutline()
    {
        var t = targetTerrain;
        var m = maskTerrain;

        var td = t.terrainData;
        var md = m.terrainData;

        if (clearTargetHolesBeforeBake)
            ClearAllHolesOnTarget();

        int holesRes = md.holesResolution;

        // original mask: true=land, false=water
        bool[,] holesRaw = md.GetHoles(0, 0, holesRes, holesRes);

        // smooth mask for nicer silhouette
        bool[,] holes = (maskSmoothIterations > 0)
            ? SmoothMaskMajority(holesRaw, holesRes, holesRes, maskSmoothIterations, maskSmoothThreshold)
            : holesRaw;

        int res = td.heightmapResolution;
        float[,] heights = td.GetHeights(0, 0, res, res);

        Vector3 tPos = t.transform.position;
        Vector3 tSize = td.size;

        Vector3 mPos = m.transform.position;
        Vector3 mSize = md.size;

        float maxSearch = Mathf.Max(2f, Mathf.Max(cliffWidth, dropoffDistance));
        float step = Mathf.Clamp(maxSearch / 18f, 0.5f, 3.0f);

        Undo.RegisterCompleteObjectUndo(td, "Bake Heights (Cliff+Dropoff+SmoothOutline)");

        int changed = 0;

        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float u = x / (float)(res - 1);
                float v = y / (float)(res - 1);

                float wx = tPos.x + u * tSize.x;
                float wz = tPos.z + v * tSize.z;

                if (wx < mPos.x || wx > mPos.x + mSize.x || wz < mPos.z || wz > mPos.z + mSize.z)
                    continue;

                if (!TryWorldToMaskIndex(mPos, mSize, holesRes, wx, wz, out int hx, out int hy))
                    continue;

                bool isLand = holes[hy, hx];
                float desiredWorldY;

                if (isLand)
                {
                    desiredWorldY = plateauY;
                }
                else
                {
                    float distToLand = DistanceToNearestLand(holes, holesRes, mPos, mSize, wx, wz, maxSearch, step);
                    if (distToLand < 0f) continue;

                    // 1) CLIFF zone: plateau -> waterline quickly
                    if (distToLand <= cliffWidth)
                    {
                        float t01 = Mathf.Clamp01(distToLand / Mathf.Max(0.001f, cliffWidth));
                        float s = Smooth01(t01, cliffSmooth);
                        desiredWorldY = Mathf.Lerp(plateauY, waterlineY, s);
                    }
                    else
                    {
                        // 2) UNDERWATER dropoff: waterline -> depth
                        float d = distToLand - cliffWidth;
                        float t01 = Mathf.Clamp01(d / Mathf.Max(0.001f, dropoffDistance));
                        float s = Smooth01(t01, dropoffSmooth);
                        desiredWorldY = Mathf.Lerp(waterlineY, dropDepthY, s);
                    }
                }

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

        if (clearTargetHolesAfterBake)
            ClearAllHolesOnTarget();

        Debug.Log($"[IslandBuilder] Bake done. Changed height samples: {changed}. MaskSmoothIters={maskSmoothIterations}, Threshold={maskSmoothThreshold}");
    }

    // --- Mask smoothing: majority filter on bool grid ---
    private static bool[,] SmoothMaskMajority(bool[,] src, int w, int h, int iterations, float threshold)
    {
        bool[,] cur = src;
        bool[,] next = new bool[h, w];

        int radius = 1; // 3x3 neighborhood

        for (int it = 0; it < iterations; it++)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int count = 0;
                    int total = 0;

                    for (int oy = -radius; oy <= radius; oy++)
                    {
                        int yy = y + oy;
                        if (yy < 0 || yy >= h) continue;

                        for (int ox = -radius; ox <= radius; ox++)
                        {
                            int xx = x + ox;
                            if (xx < 0 || xx >= w) continue;

                            total++;
                            if (cur[yy, xx]) count++;
                        }
                    }

                    float p = (total > 0) ? (count / (float)total) : 0f;
                    next[y, x] = (p >= threshold);
                }
            }

            // swap
            var tmp = cur;
            cur = next;
            next = tmp;
        }

        return cur;
    }

    private static float Smooth01(float t, float smoothness)
    {
        t = Mathf.Clamp01(t);
        float smoothstep = t * t * (3f - 2f * t);
        return Mathf.Lerp(t, smoothstep, smoothness);
    }

    private static bool TryWorldToMaskIndex(Vector3 pos, Vector3 size, int res, float wx, float wz, out int ix, out int iy)
    {
        float u = Mathf.InverseLerp(pos.x, pos.x + size.x, wx);
        float v = Mathf.InverseLerp(pos.z, pos.z + size.z, wz);
        if (u < 0f || u > 1f || v < 0f || v > 1f)
        {
            ix = iy = 0;
            return false;
        }
        ix = Mathf.Clamp(Mathf.RoundToInt(u * (res - 1)), 0, res - 1);
        iy = Mathf.Clamp(Mathf.RoundToInt(v * (res - 1)), 0, res - 1);
        return true;
    }

    // from a WATER point find nearest LAND (holes==true)
    private static float DistanceToNearestLand(bool[,] holes, int holesRes, Vector3 pos, Vector3 size, float wx, float wz, float maxRadius, float stepMeters)
    {
        if (!TryWorldToMaskIndex(pos, size, holesRes, wx, wz, out int cx, out int cy))
            return -1f;

        if (holes[cy, cx]) return 0f;

        float best = float.MaxValue;

        for (float dz = -maxRadius; dz <= maxRadius; dz += stepMeters)
        {
            for (float dx = -maxRadius; dx <= maxRadius; dx += stepMeters)
            {
                float nx = wx + dx;
                float nz = wz + dz;

                if (!TryWorldToMaskIndex(pos, size, holesRes, nx, nz, out int ix, out int iy))
                    continue;

                if (holes[iy, ix])
                {
                    float d = Mathf.Sqrt(dx * dx + dz * dz);
                    if (d < best) best = d;
                }
            }
        }

        return (best == float.MaxValue) ? -1f : best;
    }

    // ---------------- AutoPaint (fix grass under water) ----------------
    private void AutoPaintByHeight()
    {
        var t = targetTerrain;
        var td = t.terrainData;

        var layersArr = td.terrainLayers;
        int layers = layersArr != null ? layersArr.Length : 0;

        if (layers < 2)
        {
            Debug.LogError("[IslandBuilder] Target Terrain needs at least 2 TerrainLayers (Sand index 0, Grass index 1).");
            return;
        }

        if (logLayerNames)
        {
            string msg = "[IslandBuilder] TerrainLayers on Target:\n";
            for (int i = 0; i < layers; i++)
                msg += $"  [{i}] {(layersArr[i] != null ? layersArr[i].name : "NULL")}\n";
            Debug.Log(msg);
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

                int chosen;
                if (forceSandUnderWater && worldY <= waterlineY + waterlineEpsilon)
                {
                    chosen = sandLayerIndex;
                }
                else
                {
                    chosen = (worldY > grassStartY) ? grassLayerIndex : sandLayerIndex;
                }

                // force clean weights
                for (int l = 0; l < aL; l++) alpha[y, x, l] = 0f;
                alpha[y, x, chosen] = 1f;

                if (!hardCut)
                {
                    float blendBand = 0.25f;
                    float t01 = Mathf.InverseLerp(grassStartY - blendBand, grassStartY + blendBand, worldY);
                    t01 = Mathf.Clamp01(t01);

                    for (int l = 0; l < aL; l++) alpha[y, x, l] = 0f;
                    alpha[y, x, sandLayerIndex] = 1f - t01;
                    alpha[y, x, grassLayerIndex] = t01;
                }
            }
        }

        if (forceNormalizeAlphamaps)
        {
            for (int yy = 0; yy < aH; yy++)
            for (int xx = 0; xx < aW; xx++)
            {
                float sum = 0f;
                for (int l = 0; l < aL; l++) sum += alpha[yy, xx, l];

                if (sum <= 0.0001f)
                {
                    for (int l = 0; l < aL; l++) alpha[yy, xx, l] = 0f;
                    alpha[yy, xx, sandLayerIndex] = 1f;
                }
                else
                {
                    for (int l = 0; l < aL; l++) alpha[yy, xx, l] /= sum;
                }
            }
        }

        td.SetAlphamaps(0, 0, alpha);
        EditorUtility.SetDirty(td);

        Debug.Log("[IslandBuilder] AutoPaintByHeight done (no grass under water).");
    }
}