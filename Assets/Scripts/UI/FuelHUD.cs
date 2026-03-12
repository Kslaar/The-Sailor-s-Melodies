using UnityEngine;
using UnityEngine.UI;

public class FuelHUD : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BoatFuel fuel;
    [SerializeField] private Slider fuelSlider;

    [Header("Behavior")]
    [SerializeField] private bool autoFindFuel = true;
    [SerializeField] private float smoothSpeed = 10f;

    private float displayed;

    private void Awake()
    {
        if (fuelSlider != null)
        {
            fuelSlider.minValue = 0f;
            fuelSlider.maxValue = 1f;
        }
    }

    private void OnEnable()
    {
        if (fuel == null && autoFindFuel)
            fuel = FindFirstObjectByType<BoatFuel>();

        if (fuel != null && fuelSlider != null)
        {
            displayed = fuel.Normalized;
            fuelSlider.value = displayed;
        }
    }

    private void Update()
    {
        if (fuel == null && autoFindFuel)
            fuel = FindFirstObjectByType<BoatFuel>();

        if (fuel == null || fuelSlider == null) return;

        float target = fuel.Normalized;
        displayed = Mathf.Lerp(displayed, target, 1f - Mathf.Exp(-smoothSpeed * Time.unscaledDeltaTime));
        fuelSlider.value = displayed;
    }
}