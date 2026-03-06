using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(menuName ="Game/Quests/Quest")]
public class QuestAsset : ScriptableObject
{
    public string questID;
    public string title;
    [TextArea] public string description;

    public List<QuestObjective> objectives = new();
    public List<QuestReward> rewards = new();

    public enum QuestCategory { Normal, Racing }

    public QuestCategory category = QuestCategory.Normal;
}
