using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-1000)] // Damit umgehen wir das Problem, dass manche Scripte vorher gerufen werden
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public GameState State { get; private set; } = GameState.Sailing;

    public event Action<GameState, GameState> OnStateChanged;

    // Pausenmenü kann von überall geaccessed werden!
    private readonly Stack<GameState> pauseReturnStack = new();

    private static readonly Dictionary<GameState, HashSet<GameState>> Allowed = new()
    {
        {GameState.Sailing, new HashSet<GameState>{ GameState.Docked, GameState.QuestLog, GameState.Racing } },
        {GameState.Docked, new HashSet<GameState>{ GameState.Sailing, GameState.Dialogue, GameState.Racing } },
        {GameState.Dialogue, new HashSet<GameState>{ GameState.Docked } },
        {GameState.QuestLog, new HashSet<GameState>{ GameState.Sailing } },
        {GameState.Racing, new HashSet<GameState>{ GameState.Docked, GameState.Sailing } },
    };

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);    
    }

    public bool TrySetState(GameState targetState, string reason = null)
    {
        if (targetState == GameState.Paused) 
            return TryPause(reason);

        if (State == GameState.Paused)
        {
            Debug.LogWarning($"[State] Blocked transition Paused -> {targetState}. Use Unpause() instead. Reason {reason}");
            return false;
        }

        if (State == GameState.Racing && targetState == GameState.Sailing)
        {
            Debug.LogWarning("[State] Blocked Racing -> Sailing. Use TryExitRace() or finish flow.");
            return false;
        }

        if (!IsAllowed(State, targetState))
        {
            Debug.LogWarning($"[State] Blocked transition {State} -> {targetState}. Reason: {reason}");
            return false;
        }

        DoSetState(targetState, reason);
        return true;
    }

    public bool TryPause(string reason = null)
    {
        if (State == GameState.Paused) return true;

        pauseReturnStack.Push(State);
        DoSetState(GameState.Paused, reason);
        return true;
    }

    public bool Unpause(string reason = null)
    {
        if (State != GameState.Paused) 
            return false;
        
        // Check ich nicht!!!!!!!!
        var returnTo = pauseReturnStack.Count > 0 ? pauseReturnStack.Pop() : GameState.Sailing;
        DoSetState(returnTo, reason);
        return true;
    }

    public bool ToggleQuestLog(string reason = null)
    {
        if (State == GameState.QuestLog)
            return TrySetState(GameState.Sailing, reason);

        if (State != GameState.Sailing)
        {
            Debug.LogWarning($"[State] QuestLog opening blocked (must be sailingstate). Current={State}. REason {reason}");
            return false;
        }

        return TrySetState(GameState.QuestLog, reason);
    }

    ////////////////////////////////////////////////////////////////////

    public bool TryEnterDialogue(string reason = null)
    {
        if (State != GameState.Docked)
        {
            Debug.LogWarning($"[State] Dialogue opening blocked (must be in dockedstate). Current={State}. Reason: {reason}");
            return false;   
        }

        return TrySetState(GameState.Dialogue, reason);
    }

    public bool TryExitDialogue(string reason = null)
    {
        if (State != GameState.Dialogue) return false;
        return TrySetState(GameState.Docked, reason);
    }

    ////////////////////////////////////////////////////////////////////

    public bool TryEnterRace(string reason = null)
    {
        if (State != GameState.Docked)
        {
            Debug.LogWarning($"[State] Race blocked. State before must be docked. Current={State}. Reason={reason}");
            return false;
        }
        return TrySetState(GameState.Racing, reason);
    }

    public bool TryExitRace(string reason = null)
    {
        if (State != GameState.Racing) return false;
        return TrySetState(GameState.Docked, reason);
    }

    ////////////////////////////////////////////////////////////////////
    
    private bool IsAllowed(GameState from, GameState to)
    {
        if (!Allowed.TryGetValue(from, out var set)) return false;
        return set.Contains(to);
    }

    private void DoSetState(GameState newState, string reason)
    {
        if (State == newState) return;

        var old = State;
        State = newState;

        Debug.Log($"[State] {old} -> {newState}" + (string.IsNullOrEmpty(reason) ? "" : $" | {reason}"));
        OnStateChanged?.Invoke(old, newState);
    }

    ////////////////////////////////////////////////////////////////////
    
    public void ForceUnpause(GameState target = GameState.Sailing, string reason = null)
    {
        pauseReturnStack.Clear(); // Stack leeren damit wir nicht im alten State landen

        if (State == GameState.Paused)
        {
            DoSetState(target, reason ?? "ForceUnpause");
            return;
        }

        DoSetState(target, reason ?? "ForceState");
    }
}
