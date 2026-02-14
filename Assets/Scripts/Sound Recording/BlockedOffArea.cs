using UnityEngine;

public class BlockedOffArea : MonoBehaviour
{
    public Animator animator;
    public string openTriggerName = "Open";
    public Collider blockingCollider;

    public bool IsOpened { get; private set; }

    public void Open()
    {
        if (IsOpened) return;
        IsOpened= true;

        if (animator != null)
            animator.SetTrigger(openTriggerName);

        if (blockingCollider != null)
            blockingCollider.enabled = false;
    }
}
