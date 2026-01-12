using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FuelHUD : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private BoatFuel fuel;          
    [SerializeField] private Slider fuelSlider;      
    // [SerializeField] private TextMeshProUGUI fuelText;

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

    private void Update()
    {
        if (fuel == null && autoFindFuel)
            fuel = FindFirstObjectByType<BoatFuel>();

        if (fuel == null) return;

        float target = fuel.Normalized;
        displayed = Mathf.Lerp(displayed, target, 1f - Mathf.Exp(-smoothSpeed * Time.unscaledDeltaTime));

        if (fuelSlider != null)
            fuelSlider.value = displayed;
        /*
        if (fuelText != null)
            fuelText.text = $"Fuel: {fuel.CurrentFuel:0.0}s / {fuel.MaxFuel:0.0}s";
        */
    }
}
