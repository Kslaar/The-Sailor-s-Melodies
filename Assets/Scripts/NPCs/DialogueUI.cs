using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using AK.Wwise;


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
    [SerializeField] private Transform choicesRoot;
    [SerializeField] private Button choiceButtonPrefab;

    [Header("Typewriter")]
    [SerializeField] private float charactersPerSecond = 40f;

    [Header("Audio")]
    [SerializeField] private AK.Wwise.Event typeLoopStartEvent;
    [SerializeField] private AK.Wwise.Event typeLoopStopEvent;


    private Coroutine typingRoutine;
    private bool isTyping;
    private string fullText;

    // Damit der Typewriter Soundeffekt aufhört
    private uint typeLoopPlayingId = 0;

    private Action<DialogueAsset.Choice> onChoiceCallback;
    private readonly List<Button> spawnedButtons = new();

    private List<DialogueAsset.Choice> cachedChoices;

    private void Awake()
    {
        if (root != null) root.SetActive(false);
    }

    private void Update()
    {
        if (root == null || !root.activeSelf) return;

        var kb = Keyboard.current;
        var mouse = Mouse.current;

        // Skip typewriter
        if (isTyping && ((kb != null && kb.spaceKey.wasPressedThisFrame) || (mouse != null && mouse.leftButton.wasPressedThisFrame)))
        {
            FinishTypingInstant();
        }
    }

    public void Show(string npcName, Sprite avatar, string text, List<DialogueAsset.Choice> choices, Action<DialogueAsset.Choice> onChoice)
    {
        if (root != null) root.SetActive(true);

        onChoiceCallback = onChoice;

        // Header
        if (nameText != null) nameText.text = npcName ?? "";
        if (avatarImage != null)
        {
            avatarImage.sprite = avatar;
            avatarImage.enabled = avatar != null;
        }

        // Sofort Choices cachen (Dann funktioniert der Skip mid-typewriter)
        cachedChoices = choices;

        ClearChoices();
        StartTypewriter(text ?? "");
    }

    public void Hide()
    {
        StopTyping();
        StopTypeLoop();

        ClearChoices();
        cachedChoices = null;
        onChoiceCallback = null;

        if (root != null) root.SetActive(false);
    }

    private void StartTypewriter(string text)
    {
        StopTyping();
        StopTypeLoop();

        fullText = text;

        if (bodyText != null)
            bodyText.text = "";

        if (string.IsNullOrEmpty(fullText))
        {
            isTyping = false;
            typingRoutine = null;
            BuildChoices(cachedChoices);
            return;
        }
        //Loop starten
        if (typeLoopPlayingId != null)
            typeLoopPlayingId = typeLoopStartEvent.Post(gameObject);

        typingRoutine = StartCoroutine(TypeRoutine());
    }

    private IEnumerator TypeRoutine()
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

        // Zuende geschrieben
        isTyping = false;
        typingRoutine = null;

        //Loop Stoppen
        StopTypeLoop();    
        BuildChoices(cachedChoices);
    }

    private void FinishTypingInstant()
    {
        if (!isTyping) return;

        StopTyping();

        if (bodyText != null)
            bodyText.text = fullText;

        BuildChoices(cachedChoices);

        StopTypeLoop();
    }

    private void StopTyping()
    {
        if (typingRoutine != null)
            StopCoroutine(typingRoutine);

        typingRoutine = null;
        isTyping = false;

        StopTypeLoop();
    }

    private void BuildChoices(List<DialogueAsset.Choice> choices)
    {
        ClearChoices();

        if (choices == null || choices.Count == 0)
            return;

        for (int i = 0; i < choices.Count; i++)
        {
            DialogueAsset.Choice choice = choices[i];

            var btn = Instantiate(choiceButtonPrefab, choicesRoot);
            spawnedButtons.Add(btn);

            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = choice.label;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                Debug.Log($"[DialogueUI] Clicked choice: {choice.label}");
                // Wenn Spieler klickt während geschrieben wird -> instant finish
                if (isTyping)
                    FinishTypingInstant();

                onChoiceCallback?.Invoke(choice);
            });
        }
    }

    private void ClearChoices()
    {
        for (int i = 0; i < spawnedButtons.Count; i++)
        {
            if (spawnedButtons[i] != null)
                Destroy(spawnedButtons[i].gameObject);
        }
        spawnedButtons.Clear();
    }

    ////////////////////////////////////////////////////////////
    
    private void StopTypeLoop()
    {
        if (typeLoopPlayingId != 0)
        {
            AkUnitySoundEngine.StopPlayingID(typeLoopPlayingId);
            typeLoopPlayingId = 0;
        }
        else
        {
            typeLoopStopEvent?.Post(gameObject);
        }
    }
}
