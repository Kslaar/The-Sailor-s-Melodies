using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(menuName ="Game/Dialogue/DialogueAsset")]
public class DialogueAsset : ScriptableObject
{
    public Sprite npcAvatar;
    public string npcName;

    public List<DialogueNode> nodes = new();
    public string startNodeID = "start";

    [Serializable]
    public class DialogueNode
    {
        public string id;
        [TextArea(3, 8)] public string text;
        public List<Choice> choices = new();
        public List<DialogueAction> actionsOnEnter = new();
    }

    [Serializable]
    public class Choice
    {
        public string label;
        public string nextNodeID; // Falls leer = Ende
        public List<DialogueAction> actionsOnChoose = new();
    }
}
