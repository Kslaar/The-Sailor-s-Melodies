using UnityEngine;
using UnityEditor;

public class TerrainHeightClampWindow : EditorWindow
{
    private Terrain targetTerrain;
    private float maxWorldY = 1f;
    private bool includeSelectedTerrain = true;

    [MenuItem("Tools/Terrain/Clamp Max World Height")]
    
    public static void Open()
    {
        GetWindow<TerrainHeightClampWindow>("Clamp Terrain Height");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Clamp Terrain Heights (Worldspace)", EditorStyles.boldLabel);
        EditorGUILayout.Space(6);

        includeSelectedTerrain = EditorGUILayout.ToggleLeft("Auto-use selected Terrain", includeSelectedTerrain);
        if (includeSelectedTerrain)
        {
            var sel = Selection.activeGameObject;
            if (sel != null)
            {
                var t = sel.GetComponent<Terrain>();
                if (t != null) targetTerrain = t;
            }
        }

        targetTerrain = (Terrain)EditorGUILayout.ObjectField("Target Terrain", targetTerrain, typeof(Terrain), true);
        maxWorldY = EditorGUILayout.FloatField("Max World Y", maxWorldY);

        EditorGUILayout.Space(10);

        using (new EditorGUI.DisabledScope(targetTerrain == null || targetTerrain.terrainData == null))
        {
            if (GUILayout.Button("Clamp Now"))
            {
                ClampTerrain(targetTerrain, maxWorldY);
            }
        }

        EditorGUILayout.HelpBox(
            "Clamps the terrain so its surface never exceeds Max World Y.\n" +
            "Example: Terrain at Y=-10, MaxWorldY=1 -> maximum relative height = 11.",
            MessageType.Info);
    }

    private static void ClampTerrain(Terrain terrain, float maxWorldY)
    {
        if (terrain == null || terrain.terrainData == null) return;

        var td = terrain.terrainData;

        float terrainBaseY = terrain.transform.position.y;
        float maxRelative = maxWorldY - terrainBaseY;

        if (maxRelative <= 0f)
        {
            Debug.LogWarning($"[TerrainClamp] MaxWorldY ({maxWorldY}) is below Terrain base Y ({terrainBaseY}). Flattening to base.");
            FlattenTerrain(td);
            return;
        }

        float maxNormalized = Mathf.Clamp01(maxRelative / td.size.y);

        int w = td.heightmapResolution;
        int h = td.heightmapResolution;

        Undo.RegisterCompleteObjectUndo(td, "Clamp Terrain Heights");

        float[,] heights = td.GetHeights(0, 0, w, h);
        bool changed = false;

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float v = heights[y, x];
                if (v > maxNormalized)
                {
                    heights[y, x] = maxNormalized;
                    changed = true;
                }
            }
        }

        if (changed)
        {
            td.SetHeights(0, 0, heights);
            EditorUtility.SetDirty(td);
            Debug.Log($"[TerrainClamp] Done. MaxWorldY={maxWorldY} | TerrainBaseY={terrainBaseY} | MaxNormalized={maxNormalized:0.###}");
        }
        else
        {
            Debug.Log("[TerrainClamp] No heights above limit. Nothing changed.");
        }
    }

    private static void FlattenTerrain(TerrainData td)
    {
        int w = td.heightmapResolution;
        int h = td.heightmapResolution;

        Undo.RegisterCompleteObjectUndo(td, "Flatten Terrain Heights");

        float[,] heights = td.GetHeights(0, 0, w, h);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                heights[y, x] = 0f;

        td.SetHeights(0, 0, heights);
        EditorUtility.SetDirty(td);
    }
}