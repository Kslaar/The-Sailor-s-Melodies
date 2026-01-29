using System.Data.Common;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestLogInput : MonoBehaviour
{
    [SerializeField] private QuestLogUI questLogUI;

    void Update()
    {
        if (Keyboard.current == null) return;

        var gsm = GameStateManager.Instance;

        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            if (gsm == null) return;

            if (gsm.State == GameState.QuestLog)
                gsm.SetState(GameState.Sailing);
            else    
                gsm.SetState(GameState.QuestLog);
        }        
    }
}
