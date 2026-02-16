using UnityEngine;
using UnityEngine.UI;
using AK.Wwise;

public class NPCVoiceButton : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event voiceEvent;   // Das Wwise-Event für diesen NPC

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(PlayVoice);
    }

    private void PlayVoice()
    {
        if (voiceEvent != null)
        {
            voiceEvent.Post(gameObject);   // Event abspielen
        }
        else
        {
            Debug.LogWarning($"Kein Wwise Event gesetzt für {gameObject.name}");
        }
    }
}
