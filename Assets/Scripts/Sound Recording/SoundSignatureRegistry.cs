using System.Collections.Generic;
using UnityEngine;

public static class SoundSignatureRegistry
{
    private static Dictionary<string, SoundSignature> _byID;
    private static bool _initialized;

    private static void Init()
    {
        if (_initialized) return;
        _initialized = true;

        _byID = new Dictionary<string, SoundSignature>();

        var all = Resources.LoadAll<SoundSignature>("SoundSignatures");

        foreach (var sig in all)
        {
            if (sig == null) continue;
            if (string.IsNullOrWhiteSpace(sig.id)) continue;

            if (!_byID.ContainsKey(sig.id))
                _byID.Add(sig.id, sig);
        }
    }

    public static SoundSignature GetByID(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        Init();
        _byID.TryGetValue(id, out var sig);
        return sig;
    }
}