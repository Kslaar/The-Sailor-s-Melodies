using UnityEngine;

public class DockZone : MonoBehaviour
{
    public Transform snapPoint;
    public Camera dockCamera;
    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }
}
