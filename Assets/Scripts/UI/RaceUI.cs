using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class RaceUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text warningText;

    private void OnEnable()
    {
        if (RaceManager.Instance == null) return;

        RaceManager.Instance.OnCountdownChanged += OnCountdown;
        RaceManager.Instance.OnTimeChanged += OnTime;
        RaceManager.Instance.OnWarning += OnWarn;
        RaceManager.Instance.OnRaceFinished += OnFinish;
        RaceManager.Instance.OnRaceFailed += OnFail;
    }

    private void OnDisable()
    {
        if (RaceManager.Instance == null) return;

        RaceManager.Instance.OnCountdownChanged -= OnCountdown;
        RaceManager.Instance.OnTimeChanged -= OnTime;
        RaceManager.Instance.OnWarning -= OnWarn;
        RaceManager.Instance.OnRaceFinished -= OnFinish;
        RaceManager.Instance.OnRaceFailed -= OnFail;
    }

    ////////////////////////////////////////////////////
    
    private void OnCountdown(int t)
    {
        if (root != null) root.SetActive(true);
        if (countdownText != null) countdownText.text = (t == 0) ? "GO!" : t.ToString();
    }

    private void OnTime(float time)
    {
        if (timerText != null) timerText.text = time.ToString("0.00") + "s";
        if (warningText != null) warningText.text = "";
    }

    private void OnWarn(string msg)
    {
        if (warningText != null) warningText.text = msg;
    }

    private void OnFinish(string courseID, float time)
    {
        if (countdownText != null) countdownText.text = "FINISH";
    }

    private void OnFail(string reason)
    {
        if (countdownText != null) countdownText.text = "FAILED";
        if (warningText != null) warningText.text = reason;
    }
}
