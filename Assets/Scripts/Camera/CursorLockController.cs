using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(1000)]
public class CursorLockController : MonoBehaviour
{
    [Header("Mode")]
    [Tooltip("Wenn true: Cursor wird nur in Sailing/Racing gelockt. In allen anderen States ist er frei.")]
    [SerializeField] private bool lockOnlyInSailingAndRacing = true;

    [Header("Optional: Lock on Click (nur wenn Cursor im State gelockt sein soll)")]
    [SerializeField] private bool lockOnClick = false;

    [Header("Unlock Combination (nur als Debug/Notfall)")]
    [SerializeField] private Key firstKey = Key.Digit1;
    [SerializeField] private Key secondKey = Key.Digit2;
    [SerializeField] private Key thirdKey = Key.Digit3;
    [SerializeField] private float comboTimeoutSeconds = 0f;

    private int comboStep = 0;
    private float lastComboTime = 0f;

    private CursorLockMode appliedLockState = (CursorLockMode)(-1);
    private bool appliedVisible = true;

    private void OnEnable()
    {
        ApplyForCurrentState(force: true);

        var gsm = GameStateManager.Instance;
        if (gsm != null) gsm.OnStateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        var gsm = GameStateManager.Instance;
        if (gsm != null) gsm.OnStateChanged -= OnStateChanged;
    }

    private void OnStateChanged(GameState from, GameState to)
    {
        if (to == GameState.Paused || to == GameState.Dialogue || to == GameState.Docked || to == GameState.QuestLog)
            ResetCombo();

        ApplyForCurrentState(force: true);
    }

    private void LateUpdate()
    {
        ApplyForCurrentState(force: false);
    }

    private void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;
        if (kb == null) return;

        if (lockOnClick && mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (ShouldBeLockedInCurrentState())
            {
                LockCursor();
                ResetCombo();
            }
        }

        if (comboTimeoutSeconds > 0f && comboStep > 0)
        {
            if (Time.unscaledTime - lastComboTime > comboTimeoutSeconds)
                ResetCombo();
        }

        if (WasPressed(kb, firstKey)) AdvanceCombo(expectedStep: 0);
        else if (WasPressed(kb, secondKey)) AdvanceCombo(expectedStep: 1);
        else if (WasPressed(kb, thirdKey)) AdvanceCombo(expectedStep: 2);
    }

    private void ApplyForCurrentState(bool force)
    {
        bool shouldLock = ShouldBeLockedInCurrentState();

        if (shouldLock) LockCursor(force);
        else UnlockCursor(force);
    }

    private bool ShouldBeLockedInCurrentState()
    {
        if (!lockOnlyInSailingAndRacing) return Cursor.lockState == CursorLockMode.Locked;

        var gsm = GameStateManager.Instance;
        if (gsm == null) return false;

        return gsm.State == GameState.Sailing || gsm.State == GameState.Racing;
    }

    private bool WasPressed(Keyboard kb, Key key)
    {
        return key switch
        {
            Key.Digit1 => kb.digit1Key.wasPressedThisFrame,
            Key.Digit2 => kb.digit2Key.wasPressedThisFrame,
            Key.Digit3 => kb.digit3Key.wasPressedThisFrame,
            _ => false
        };
    }

    private void AdvanceCombo(int expectedStep)
    {
        if (comboStep != expectedStep)
        {
            ResetCombo();
            comboStep = (expectedStep == 0) ? 1 : 0;
        }
        else
        {
            comboStep++;
        }

        lastComboTime = Time.unscaledTime;

        if (comboStep >= 3)
        {
            UnlockCursor(force: true);
            ResetCombo();
        }
    }

    private void ResetCombo()
    {
        comboStep = 0;
        lastComboTime = 0f;
    }

    public void LockCursor(bool force = true)
    {
        if (!force && appliedLockState == CursorLockMode.Locked && appliedVisible == false) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        appliedLockState = CursorLockMode.Locked;
        appliedVisible = false;
    }

    public void UnlockCursor(bool force = true)
    {
        if (!force && appliedLockState == CursorLockMode.None && appliedVisible == true) return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        appliedLockState = CursorLockMode.None;
        appliedVisible = true;
    }
}
