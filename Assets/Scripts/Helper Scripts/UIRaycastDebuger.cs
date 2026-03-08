using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIRaycastDebugger : MonoBehaviour
{
    private readonly List<RaycastResult> _results = new();

    void Update()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        var es = EventSystem.current;
        if (es == null)
        {
            Debug.LogWarning("[UIRaycast] No EventSystem in scene!");
            return;
        }

        var ped = new PointerEventData(es)
        {
            position = Mouse.current.position.ReadValue()
        };

        _results.Clear();
        es.RaycastAll(ped, _results);

        if (_results.Count == 0)
        {
            return;
        }

        for (int i = 0; i < Mathf.Min(5, _results.Count); i++)
            Debug.Log($"  #{i} hit: {_results[i].gameObject.name}", _results[i].gameObject);
    }
}
