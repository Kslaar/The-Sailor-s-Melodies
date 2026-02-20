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

    private DockZone _currentDock;
    private float _holdTimer;
    private bool _isDocked;

    // private Vector3 savedPos;
    // private Quaternion savedRot;
    private bool _savedKinematic;

    public bool IsDocked => _isDocked;
    public DockZone CurrentDock => _currentDock;

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

        if (!_isDocked)
        {
            if (_currentDock == null) return;

            if (Keyboard.current.qKey.isPressed)
            {
                _holdTimer += Time.deltaTime;
                if (_holdTimer >= holdsSecondsToDock) 
                    DockNow();
            }
            else
            {
                _holdTimer = 0f;
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
        if (_currentDock == null) return;
        _isDocked = true;
        _holdTimer = 0f;

        // savedPos = transform.position;
        // savedRot = transform.rotation;
        _savedKinematic = rb.isKinematic;

        // Boot wird ans Dock gesnapped
        transform.position = _currentDock.snapPoint.position;
        transform.rotation = _currentDock.snapPoint.rotation;

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
        if (_currentDock.dockCamera != null) _currentDock.dockCamera.gameObject.SetActive(true);

        if (_currentDock.dockUI != null)
            _currentDock.dockUI.Show(_currentDock);

        GameStateManager.Instance?.TrySetState(GameState.Docked);

        // WWise: MusicState setzen
        GlobalMusicManager.Instance.SetIsland();
    }

    /*
    public void ForceDock(DockZone dockZone, string reason = "ForceDock", bool showDockUI = true)
    {
        if (dockZone == null || dockZone.snapPoint == null)
        {
            Debug.LogWarning("[BoatDockingController] ForceDock failed: dockZone/snapPoint missing");
            return;
        }

        var gsm = GameStateManager.Instance;

        var oldDock = _currentDock;
        if (oldDock != null && oldDock != dockZone && oldDock.dockCamera != null)
            oldDock.dockCamera.gameObject.SetActive(false);

        _currentDock = dockZone;
        _isDocked = true;
        _holdTimer = 0f;

        if (rb != null) _savedKinematic = rb.isKinematic;

        transform.position = dockZone.snapPoint.position;
        transform.rotation = dockZone.snapPoint.rotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (boat != null) boat.enabled = false;

        // Wir geben den Cursor wieder frei
        if (cursorLock != null) cursorLock.UnlockCursor();
        else { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

        // Kameras switchen
        if (sailingCamera != null) sailingCamera.gameObject.SetActive(false);
        if (_currentDock.dockCamera != null) _currentDock.dockCamera.gameObject.SetActive(true);
        else Debug.LogWarning($"[BoatDockingController] DockCamera missing on {dockZone.name}");

        if (dockZone.dockUI != null)
        {
            if (showDockUI) dockZone.dockUI.Show(dockZone);
            else dockZone.dockUI.Hide();
        }
        
        if (gsm != null)
            gsm.ForceUnpause(GameState.Docked, reason);

        GlobalMusicManager.Instance.SetIsland();
    }
    */

    public void UndockNow()
    {
        if (!_isDocked) return;

        var gsm = GameStateManager.Instance;

        if (gsm != null && gsm.State == GameState.Dialogue)
            gsm.TryExitDialogue("Undock while in Dialogue...");

        _isDocked = false;

        // Gleiches wie beim Docken nur reversed:
        if (_currentDock != null && _currentDock.dockCamera != null) _currentDock.dockCamera.gameObject.SetActive(false);
        if (sailingCamera != null) sailingCamera.gameObject.SetActive(true);

        rb.isKinematic = _savedKinematic;
        if (boat != null) boat.enabled = true;

        if (cursorLock != null) cursorLock.LockCursor();
        else { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }

        if (_currentDock != null && _currentDock.dockUI != null)
            _currentDock.dockUI.Hide();

        if (gsm != null && gsm.State != GameState.Racing)
            gsm.TrySetState(GameState.Sailing, "Undock Now");

        // Wwise State setzen
        GlobalMusicManager.Instance.SetExploration();


    }

    public void UndockForRace()
    {
        if (!_isDocked) return;

        _isDocked = false;

        // Kameras zurück
        if (_currentDock != null && _currentDock.dockCamera != null)
            _currentDock.dockCamera.gameObject.SetActive(false);
        if (sailingCamera != null) 
            sailingCamera.gameObject.SetActive(true);

        // Physik/Steuerung zurück
        rb.isKinematic = _savedKinematic;
        if (boat != null) boat.enabled = true;

        // Cursor wieder locken
        if (cursorLock != null) cursorLock.LockCursor();
        else { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }

        // Dock UI schließen
        if (_currentDock != null && _currentDock.dockUI != null)
            _currentDock.dockUI.Hide();
    }

    public void AutoDockForDialogue(DockZone dockZone, DialogueAsset dialogue, string reason = "AutoDockForDialogue", bool showDockUI = false)
    {
        if (dockZone == null || dockZone.snapPoint == null)
        {
            Debug.LogWarning("[BoatDockingController] AutoDockForDialogue failed: dockZone/snapPoint missing");
            return;
        }

        if (dialogue == null)
        {
            Debug.LogWarning("[BoatDockingController] AutoDockForDialogue failed: dialogue is NULL");
            return;
        }

        var gsm = GameStateManager.Instance;
        if (gsm == null)
        {
            Debug.LogWarning("[BoatDockingController] AutoDockForDialogue failed: dialogue is NULL");
            return;
        }

        if (gsm.State == GameState.Racing)
            gsm.TryExitRace("AutoDockForDialogue");

        if (CurrentDock != null && CurrentDock != dockZone && CurrentDock.dockCamera != null)
            CurrentDock.dockCamera.gameObject.SetActive(false);

        _currentDock = dockZone;
        _isDocked = true;
        _holdTimer = 0f;

        if (rb != null) _savedKinematic = rb.isKinematic;

        transform.position = dockZone.snapPoint.position;
        transform.rotation = dockZone.snapPoint.rotation;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (boat != null) boat.enabled = false;

        // Wir geben den Cursor wieder frei
        if (cursorLock != null) cursorLock.UnlockCursor();
        else { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        
        if (sailingCamera != null) sailingCamera.gameObject.SetActive(false);
        if (dockZone.dockCamera != null) dockZone.dockCamera.gameObject.SetActive(true);

        if (dockZone.dockUI != null)
        {
            if (showDockUI) dockZone.dockUI.Show(dockZone);
            else dockZone.dockUI.Hide();
        }

        gsm.ForceUnpause(GameState.Docked, reason);

        bool entered = gsm.TryEnterDialogue(reason);
        if (!entered)
        {
            Debug.LogWarning($"[BoatDockingController] TryEnterDialogue blocked. CurrentState={gsm.State}. Forcing Docked then retry.");
            gsm.ForceUnpause(GameState.Docked, reason + " ForceDocked");
            gsm.TryEnterDialogue(reason + " Retry");
        }

        DialogueManager.Instance?.StartDialogue(dialogue);
    }

    /////////////////////////////////////////////////////////////

    private void OnTriggerEnter(Collider other)
    {
        var dock = other.GetComponentInParent<DockZone>();
        if (dock != null) _currentDock = dock;
    }

    private void OnTriggerExit(Collider other)
    {
        var dock = other.GetComponentInParent<DockZone>();
        if (dock != null && dock == _currentDock) _currentDock = null;      
    }
}
