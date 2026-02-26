using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class RaceUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text countdownText;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text warningText;

    [SerializeField] private float goTextVisible = 1f;

    private Coroutine goTimer;

    private void OnEnable()
    {
        if (RaceManager.Instance == null) return;

        RaceManager.Instance.OnCountdownChanged += OnCountdown;
        RaceManager.Instance.OnTimeChanged += OnTime;
        RaceManager.Instance.OnWarning += OnWarn;
        RaceManager.Instance.OnRaceFinished += OnFinish;
        RaceManager.Instance.OnRaceFailed += OnFail;

        if (root != null) root.SetActive(true);
        if (countdownText != null) countdownText.text = "";
        if (timerText != null) timerText.text = "";
        if (warningText != null) warningText.text = "";
    }

    private void OnDisable()
    {
        if (RaceManager.Instance == null) return;

        RaceManager.Instance.OnCountdownChanged -= OnCountdown;
        RaceManager.Instance.OnTimeChanged -= OnTime;
        RaceManager.Instance.OnWarning -= OnWarn;
        RaceManager.Instance.OnRaceFinished -= OnFinish;
        RaceManager.Instance.OnRaceFailed -= OnFail;

        if (goTimer != null) StopCoroutine(goTimer);
        goTimer = null;
    }

    ////////////////////////////////////////////////////
    
    private void OnCountdown(int t)
    {
       if (root != null) root.SetActive(true);

        // Während Countdown: Warnung sicher weg
        if (warningText != null) warningText.text = "";

        if (countdownText == null) return;

        // alte GO-Coroutine stoppen
        if (goTimer != null)
        {
            StopCoroutine(goTimer);
            goTimer = null;
        }

        if (t == 0)
        {
            countdownText.text = "GO!";
            goTimer = StartCoroutine(CoruClearGoAfterDelay());
        }
        else
        {
            countdownText.text = t.ToString();
        }
    }

    private IEnumerator CoruClearGoAfterDelay()
    {
        yield return new WaitForSecondsRealtime(goTextVisible);
        if (countdownText != null && countdownText.text == "GO!")
            countdownText.text = "";
        goTimer = null;
    }

    private void OnTime(float time)
    {
        if (timerText != null) timerText.text = RaceTime(time);
        if (warningText != null) warningText.text = "";
    }

    private void OnWarn(string msg)
    {
        if (warningText == null) return;

        if (string.IsNullOrWhiteSpace(msg))
        {
            warningText.text = "";
            return;
        }
        warningText.text = msg;
    }

    private static string RaceTime(float timeSeconds)
    {
        if (timeSeconds < 0f) timeSeconds = 0f;

        int totalMs = Mathf.FloorToInt(timeSeconds * 1000f);
        int minutes = totalMs / 60000;
        int seconds = (totalMs / 1000) % 60;
        int ms = totalMs % 1000;

        return $"{minutes}:{seconds:00}.{ms:00}";
    }

    private void OnFinish(string courseID, float time)
    {
        if (countdownText != null) countdownText.text = "FINISH";
        if (warningText != null) warningText.text = "";
    }

    private void OnFail(string reason)
    {
        if (countdownText != null) countdownText.text = "FAILED";
        if (warningText != null) warningText.text = reason;
    }
}
