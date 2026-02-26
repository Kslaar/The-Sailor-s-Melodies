using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SignatureToEvent
{
    public SoundSignature signature;
    public AK.Wwise.Event hintEvent;
}