using UnityEngine;
using UnityEngine.InputSystem;

public class KeyboardInput : MonoBehaviour
{
    public KeyboardKey keyC;
    public KeyboardKey keyD;
    public KeyboardKey keyE;
    public KeyboardKey keyF;
    public KeyboardKey keyG;
    public KeyboardKey keyA;
    public KeyboardKey keyB;

    void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame) keyC.PlayNote();
        if (Keyboard.current.zKey.wasPressedThisFrame) keyD.PlayNote();
        if (Keyboard.current.uKey.wasPressedThisFrame) keyE.PlayNote();
        if (Keyboard.current.iKey.wasPressedThisFrame) keyF.PlayNote();
        if (Keyboard.current.oKey.wasPressedThisFrame) keyG.PlayNote();
        if (Keyboard.current.pKey.wasPressedThisFrame) keyA.PlayNote();
        if (Keyboard.current.rKey.wasPressedThisFrame) keyB.PlayNote();
    }
}