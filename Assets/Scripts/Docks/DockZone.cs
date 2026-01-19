using UnityEngine;

public class DockZone : MonoBehaviour
{
    public Transform snapPoint;
    public Camera dockCamera;

    [Header("Dock Interactions")]
    public DockInteractionUI dockUI;
    public DialogueAsset defaultDialogue;
    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }
}
