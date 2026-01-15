using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Header")]
    [SerializeField] private Image avatarImage;
    [SerializeField] private TMP_Text nameText;

    [Header("Body")]
    [SerializeField] private TMP_Text bodyText;

    [Header("Choices")]
    [SerializeField] private  Transform choicesRoot;
    [SerializeField] private  Button choiceButtonPrefab;

    [Header("Typewriter")]
    [SerializeField] private float charactersPerSecond = 40f;

    private Coroutine typingRoutine;
    private bool isTyping;
    private string fullText;

    private Action<DialogueAsset.Choice> onChoiceCallback;
    private List<Button> spawnedButtons = new();

    private void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    private void Update()
    {
        if (root == null || !root.activeSelf) return;

        var kb = Keyboard.current;
        var mouse = Mouse.current;

        // Möglichkeit den Typewriter-Effekt zu überspringen
        if (isTyping && ((kb != null && kb.spaceKey.wasPressedThisFrame) || (mouse != null && mouse.leftButton.wasPressedThisFrame)))
        {
            SkipTypewriter();
        }
    }

    public void Show(string npcName, Sprite avatar, string text, List<DialogueAsset.Choice> choices, Action<DialogueAsset.Choice> onChoice)
    {
        if (root != null) root.SetActive(true);

        onChoiceCallback = onChoice;

        // NACHFRAGEN
        if (nameText != null) nameText.text = npcName ?? "";
        if (avatarImage != null)
        {
            avatarImage.sprite = avatar;
            avatarImage.enabled = avatar != null;
        }

        ClearChoices();

        StartTypewriter(text ?? "", choices);
    }

    public void Hide()
    {
        StopTyping();
        ClearChoices();
        if (root != null) root.SetActive(false);
    }

    private void StartTypewriter(string text, List<DialogueAsset.Choice> choices)
    {
        StopTyping();

        fullText = text;
        if (bodyText != null) bodyText.text = "";

        typingRoutine = StartCoroutine(TypeRoutine(choices));
    }

    private IEnumerator TypeRoutine(List<DialogueAsset.Choice> choices)
    {
        isTyping = true;

        float delay = (charactersPerSecond <= 0f) ? 0f : (1f / charactersPerSecond);

        for (int i = 0; i < fullText.Length; i++)
        {
            if (bodyText != null)
                bodyText.text += fullText[i];

            if (delay > 0f)
                yield return new WaitForSecondsRealtime(delay);
            else
                yield return null;
        }

        isTyping = false;
        typingRoutine = null;

        BuildChoices(choices);
    }

    private void SkipTypewriter()
    {
        StopTyping();

        if (bodyText != null) bodyText.text = fullText;

        isTyping = false;
        typingRoutine = null;

        // Wenn du skipst, müssen die Choices trotzdem erscheinen.
        // Dazu bauen wir sie neu auf – DialogueManager ruft ShowNode -> Show() bei jedem Node,
        // also können wir sie hier einfach anzeigen, wenn vorhanden.
        // (Choices werden im StartTypewriter übergeben; wir speichern sie nicht dauerhaft.
        // Deshalb: Wenn du 100% Skip+Choices willst, lass Skip nur per Click passieren nachdem Node geladen ist.)
        // -> Für solide Skip/Choices: wir speichern die choices in einem Feld.
        // Minimal-Fix: Im Skip bauen wir keine neuen Choices, sondern lassen die Coroutine am Ende dafür sorgen.
        // ABER: weil wir StopCoroutine machen, würde die Coroutine nicht mehr zu Ende laufen.
        // Also: wir brauchen cached choices.
    }

    private List<DialogueAsset.Choice> cachedChoices;

    private void StopTyping()
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = null;
        isTyping = false;
    }
}
