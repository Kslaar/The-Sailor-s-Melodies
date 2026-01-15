using UnityEngine;
using UnityEngine.InputSystem;

public class BoatDockingController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoatControl boat;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private CursorLockController cursorLock;
    [SerializeField] private Camera sailingCamera;

    [Header("Docking")]
    [SerializeField] private float holdsSecondsToDock = 0.7f;

    private DockZone currentDock;
    private float holdTimer;
    private bool isDocked;

    private Vector3 savedPos;
    private Quaternion savedRot;
    private bool savedKinematic;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        boat = GetComponent<BoatControl>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        if (GameStateManager.Instance != null && 
            GameStateManager.Instance.State == GameState.Dialogue || GameStateManager.Instance.State == GameState.QuestLog)
        return;

        if (!isDocked)
        {
            if (currentDock == null) return;

            if (Keyboard.current.qKey.isPressed)
            {
                holdTimer += Time.deltaTime;
                if (holdTimer >= holdsSecondsToDock) 
                    DockNow();
            }
            else
            {
                holdTimer = 0f;
            }
        }
        else
        {
            if (Keyboard.current.qKey.wasPressedThisFrame)
                UndockNow();
        }
    }

    private void DockNow()
    {
        if (currentDock == null) return;
        isDocked = true;
        holdTimer = 0f;

        // Wo waren wir vor dem Andocken? => Gespeichert
        savedPos = transform.position;
        savedRot = transform.rotation;
        savedKinematic = rb.isKinematic;

        //Boot wird ans Dock gesnapped
        transform.position = currentDock.snapPoint.position;
        transform.rotation = currentDock.snapPoint.rotation;

        // Wir deaktivieren die Physik und Steuerung des Bootes
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        if (boat != null) boat.enabled = false;

        // Wir geben den Cursor wieder frei
        if (cursorLock != null) cursorLock.UnlockCursor();
        else { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

        // Kameras switchen
        if (sailingCamera != null) sailingCamera.gameObject.SetActive(false);
        if(currentDock.dockCamera != null) currentDock.dockCamera.gameObject.SetActive(true);

        GameStateManager.Instance?.SetState(GameState.Docked);
    }

    public void UndockNow()
    {
        if (!isDocked) return;
        isDocked = false;

        // Gleiches wie beim Docken nur reversed:
        if (currentDock != null && currentDock.dockCamera != null) currentDock.dockCamera.gameObject.SetActive(false);
        if (sailingCamera != null) sailingCamera.gameObject.SetActive(true);

        rb.isKinematic = savedKinematic;
        if (boat != null) boat.enabled = true;

        if (cursorLock != null) cursorLock.LockCursor();
        else { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }

        GameStateManager.Instance?.SetState(GameState.Sailing);
    }

    private void OnTriggerEnter(Collider other)
    {
        var dock = other.GetComponentInParent<DockZone>();
        if (dock != null) currentDock = dock;
    }

    private void OnTriggerExit(Collider other)
    {
        var dock = other.GetComponentInParent<DockZone>();
        if (dock != null && dock == currentDock) currentDock = null;      
    }
}
