using System;
using UnityEngine;

public class QuitWatcher : MonoBehaviour
{
    private void OnEnable()
    {
        Application.wantsToQuit += OnWantsToQuit;
        Application.quitting += OnQuitting;
    }

    private void OnDisable()
    {
        Application.wantsToQuit -= OnWantsToQuit;
        Application.quitting -= OnQuitting;
    }

    private bool OnWantsToQuit()
    {
        Debug.LogError("[QUIT WATCHER] Application.wantsToQuit\n" + Environment.StackTrace);
        return true;
    }

    private void OnQuitting()
    {
        Debug.LogError("[QUIT WATCHER] Application.quitting\n" + Environment.StackTrace);
    }
}