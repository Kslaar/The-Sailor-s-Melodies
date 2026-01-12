using UnityEngine;
using UnityEngine.InputSystem;

public class CursorLockController : MonoBehaviour
{
    [Header("Lock Settings")]
    [SerializeField] private bool lockOnClick = true;
    [SerializeField] private bool lockAtStart = false;

    [Header("Unlock Combination")]
    [SerializeField] private Key firstKey = Key.Digit1;
    [SerializeField] private Key secondKey = Key.Digit2;
    [SerializeField] private Key thirdKey = Key.Digit3;

    [SerializeField] private float comboTimeoutSeconds = 0f; // damit die Kombination wieder zurückgesetzt wird
    private int comboStep = 0;
    private float lastComboTime = 0f;

    void Start()
    {
        if (lockAtStart) 
            LockCursor();
        else 
            UnlockCursor();
    }

    void Update()
    {
        var kb = Keyboard.current;
        var mouse = Mouse.current;

        if (kb == null) return;

        if (!IsLocked() && lockOnClick && mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            LockCursor();
            ResetCombo();
        }

        if (comboTimeoutSeconds > 0f && comboStep > 0)
        {
            if (Time.unscaledTime - lastComboTime > comboTimeoutSeconds)
                ResetCombo();
        }

        // Kombination
        if (WasPressed(kb, firstKey))
            AdvanceCombo(expectedStep: 0);
        else if (WasPressed(kb, secondKey))
            AdvanceCombo(expectedStep: 1);
        else if (WasPressed(kb, thirdKey))
            AdvanceCombo(expectedStep: 2);
    }

    private bool WasPressed(Keyboard kb, Key key)
    {
        if (key == Key.Digit1) return kb.digit1Key.wasPressedThisFrame;
        if (key == Key.Digit2) return kb.digit2Key.wasPressedThisFrame;
        if (key == Key.Digit3) return kb.digit3Key.wasPressedThisFrame;
        return false;
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
            UnlockCursor();
            ResetCombo();
        }
    }

    private bool IsLocked() => Cursor.lockState == CursorLockMode.Locked;

    public void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void ResetCombo()
    {
        comboStep = 0;
        lastComboTime = 0f;
    }
}
