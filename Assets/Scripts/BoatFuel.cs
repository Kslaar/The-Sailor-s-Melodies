using UnityEngine;

public class BoatFuel : MonoBehaviour
{
    [SerializeField] private float maxFuelSeconds = 30f;
    [SerializeField] private float currentFuelSeconds = 30f;

    public float MaxFuel => maxFuelSeconds;
    public float CurrentFuel => currentFuelSeconds;
    public float Normalized => maxFuelSeconds <= 0 ? 0 : currentFuelSeconds / maxFuelSeconds;

    public void SetMaxFuel(float seconds, bool fill = true)
    {
        maxFuelSeconds = Mathf.Max(0f, seconds);

        if (fill) currentFuelSeconds = maxFuelSeconds;
        else currentFuelSeconds = Mathf.Clamp(currentFuelSeconds, 0f, maxFuelSeconds);
    }

    public void Refill() => currentFuelSeconds = maxFuelSeconds;

    public void AddFuel(float seconds)
    {
        currentFuelSeconds = Mathf.Clamp(currentFuelSeconds + Mathf.Max(0f, seconds), 0f, maxFuelSeconds);
    }

    public bool HasFuel(float min = 0.001f) => currentFuelSeconds > min;

    public bool UseFuel(float seconds)
    {
        if (seconds <= 0f) return true;
        if (currentFuelSeconds <= 0f) return false;

        currentFuelSeconds = Mathf.Max(0f, currentFuelSeconds - seconds);
        return true;
    }
}
