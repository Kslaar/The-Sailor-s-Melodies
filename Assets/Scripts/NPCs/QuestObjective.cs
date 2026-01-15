using UnityEngine;

public abstract class QuestObjective : ScriptableObject
{
    public abstract void Register();
    public abstract void Unregister();
    public abstract bool IsComplete { get; }
}
