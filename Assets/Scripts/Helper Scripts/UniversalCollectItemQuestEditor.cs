#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UniversalCollectItemQuest))]
public class UniversalCollectItemQuestEditor : Editor
{
    SerializedProperty modeProp;
    SerializedProperty itemIDProp;
    SerializedProperty requiredCountProp;
    SerializedProperty requiredItemIDsProp;

    void OnEnable()
    {
        modeProp = serializedObject.FindProperty("mode");
        itemIDProp = serializedObject.FindProperty("itemID");
        requiredCountProp = serializedObject.FindProperty("requiredCount");
        requiredItemIDsProp = serializedObject.FindProperty("requiredItemIDs");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(modeProp);

        var mode = (UniversalCollectItemQuest.Mode)modeProp.enumValueIndex;

        EditorGUILayout.Space(8);

        if (mode == UniversalCollectItemQuest.Mode.CountSingleItem)
        {
            EditorGUILayout.LabelField("CountSingleItem", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(itemIDProp);
            EditorGUILayout.PropertyField(requiredCountProp);
        }
        else if (mode == UniversalCollectItemQuest.Mode.CollectUniqueIDs)
        {
            EditorGUILayout.LabelField("CollectUniqueIDs", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(requiredItemIDsProp, true);

            EditorGUILayout.HelpBox("Jede ID zählt nur einmal (unique).", MessageType.Info);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
