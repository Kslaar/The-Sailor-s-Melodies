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

    // private Vector3 savedPos;
    // private Quaternion savedRot;
    private bool savedKinematic;

    public bool IsDocked => isDocked;
    public DockZone CurrentDock => currentDock;

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        boat = GetComponent<BoatControl>();
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        var gsm = GameStateManager.Instance;

        if (gsm != null && (gsm.State == GameState.Dialogue || gsm.State == GameState.QuestLog || gsm.State == GameState.Paused || gsm.State == GameState.Racing))
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

        // savedPos = transform.position;
        // savedRot = transform.rotation;
        savedKinematic = rb.isKinematic;

        // Boot wird ans Dock gesnapped
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
        if (currentDock.dockCamera != null) currentDock.dockCamera.gameObject.SetActive(true);

        /*DialogueAsset dialogueToUse = currentDock.defaultDialogue;
        
        var selector = currentDock.GetComponent<NPCDialogueSelector>();
        if (selector != null)
            dialogueToUse = selector.GetDialogue();*/

        if (currentDock.dockUI != null)
            currentDock.dockUI.Show(currentDock);

        GameStateManager.Instance?.TrySetState(GameState.Docked);
    }

    public void UndockNow()
    {
        if (!isDocked) return;

        var gsm = GameStateManager.Instance;

        if (gsm != null && gsm.State == GameState.Dialogue)
            gsm.TryExitDialogue("Undock while in Dialogue...");

        isDocked = false;

        // Gleiches wie beim Docken nur reversed:
        if (currentDock != null && currentDock.dockCamera != null) currentDock.dockCamera.gameObject.SetActive(false);
        if (sailingCamera != null) sailingCamera.gameObject.SetActive(true);

        rb.isKinematic = savedKinematic;
        if (boat != null) boat.enabled = true;

        if (cursorLock != null) cursorLock.LockCursor();
        else { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }

        if (currentDock != null && currentDock.dockUI != null)
            currentDock.dockUI.Hide();

        gsm?.TrySetState(GameState.Sailing, "Undock Now");
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
