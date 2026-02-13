using UnityEngine;

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
if (Input.GetKeyDown(KeyCode.A)) keyC.PlayNote();
if (Input.GetKeyDown(KeyCode.S)) keyD.PlayNote();
if (Input.GetKeyDown(KeyCode.D)) keyE.PlayNote();
if (Input.GetKeyDown(KeyCode.F)) keyF.PlayNote();
if (Input.GetKeyDown(KeyCode.G)) keyG.PlayNote();
if (Input.GetKeyDown(KeyCode.H)) keyA.PlayNote();
if (Input.GetKeyDown(KeyCode.J)) keyB.PlayNote();





}
}
