using UnityEngine;

public class SequenceCompleteEffects : MonoBehaviour
{
    [Header("Which sequence triggers these effects?")]
    public string sequenceID = "secret_sequence";

    [Header("Rock Animation")]
    public Animator rockAnimator;
    public string rockBoolParam = "Cor_Combination"; // Parameter
    public bool rockBoolValue = true;

    [Header("Camera Shake Animation")]
    public Animator cameraAnimator;
    public string cameraTriggerParam = "Shake";      // oder Bool-Param Name
    public bool useCameraTrigger = true;             // true=Trigger, false=Bool

    [Header("Wwise Earthquake")]
    public AK.Wwise.Event earthquakeEvent;
    public GameObject wwiseEmitter;

    private void OnEnable()
    {
        SecretSequenceObjective.OnSequenceObjectiveCompleted += OnCompleted;
    }

    private void OnDisable()
    {
        SecretSequenceObjective.OnSequenceObjectiveCompleted -= OnCompleted;
    }

    private void OnCompleted(string completedSequenceID)
    {
        if (!string.Equals(completedSequenceID, sequenceID, System.StringComparison.OrdinalIgnoreCase))
            return;

        // 1) Rock
        if (rockAnimator != null && !string.IsNullOrEmpty(rockBoolParam))
            rockAnimator.SetBool(rockBoolParam, rockBoolValue);

        // 2) Camera shake
        if (cameraAnimator != null)
        {
            if (useCameraTrigger && !string.IsNullOrEmpty(cameraTriggerParam))
                cameraAnimator.SetTrigger(cameraTriggerParam);
        }

        // 3) Wwise earthquake
        if (earthquakeEvent != null)
        {
            var emitter = (wwiseEmitter != null) ? wwiseEmitter : gameObject;
            earthquakeEvent.Post(emitter);
        }
    }
}