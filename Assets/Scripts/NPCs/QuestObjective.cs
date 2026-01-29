using UnityEngine;

public abstract class QuestObjective : ScriptableObject
{
    public abstract void Register();
    public abstract void Unregister();
    public virtual string ProgressText => IsComplete ? "Done" : "";
    public virtual float Progress01 => IsComplete ? 1f : 0f;
    public abstract bool IsComplete { get; }
}
