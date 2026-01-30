using System.Data.Common;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestLogInput : MonoBehaviour
{
    // [SerializeField] private QuestLogUI questLogUI;

    void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.tKey.wasPressedThisFrame)
            return;

        var gsm = GameStateManager.Instance;
        if (gsm == null) return;
    
        if (gsm.State != GameState.Sailing && gsm.State != GameState.QuestLog)
            return;
   
        gsm.ToggleQuestLog("Pressed T");       
    }
}
