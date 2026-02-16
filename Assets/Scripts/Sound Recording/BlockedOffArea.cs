using UnityEngine;

public class BlockedOffArea : MonoBehaviour
{
    public Animator animator;
    public string openTriggerName = "Open";
    public Collider blockingCollider;

    public string closedLayer = "Obstacle";
    public string openedLayer = "Default"; // vllt überarbeiten
    public bool applyToChildren = true;

    public bool disableColliderOnOpen = true;

    public bool IsOpened { get; private set; }

    void Awake()
    {
        if (!IsOpened)
        {
            SetLayerByName(closedLayer, applyToChildren);

            if (blockingCollider != null)
                blockingCollider.enabled = true;
        }
    }
    public void Open()
    {
        if (IsOpened) return;
        IsOpened= true;

        if (animator != null)
            animator.SetTrigger(openTriggerName);

        if (blockingCollider != null && disableColliderOnOpen)
            blockingCollider.enabled = false;
    }

    public void SetLayerByName(string layerName, bool includeChildren)
    {
        int layer = LayerMask.NameToLayer(layerName);
        if (layer == -1)
            return;

        if (!includeChildren)
        {
            gameObject.layer = layer;
            return;
        }

        foreach (Transform t in GetComponentInChildren<Transform>(true))
            t.gameObject.layer = layer;
    }
}
