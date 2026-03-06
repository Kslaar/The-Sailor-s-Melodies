using UnityEngine;

public class QuestSoundListenerZone : MonoBehaviour
{
    [Header("Setup")]
    public RecordHotbar recorder;
    public string listenerID = "npc";

    private bool _playerInRange;

    private void OnEnable()
    {
        if (recorder != null) recorder.OnSoundPlayed += HandlePlayed;
    }

    private void OnDisable()
    {
        if (recorder != null) recorder.OnSoundPlayed -= HandlePlayed;
    }

    private void HandlePlayed(SoundSignature sig)
    {
        if (!_playerInRange) return;
        QuestPlaybackEvents.RaisePlayedNearListener(listenerID, sig);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            _playerInRange = false;
    }
}