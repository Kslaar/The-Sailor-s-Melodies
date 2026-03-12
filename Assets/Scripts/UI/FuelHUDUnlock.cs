using UnityEngine;

public class FuelHUDUnlock : MonoBehaviour
{
    [SerializeField] private GameObject fuelHudRoot;

    private void Start()
    {
        Refresh();
    }

    private void OnEnable()
    {
        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.OnProgressionChanged += Refresh;

        Refresh();
    }

    private void OnDisable()
    {
        if (ProgressionManager.Instance != null)
            ProgressionManager.Instance.OnProgressionChanged -= Refresh;
    }

    private void Refresh()
    {
        if (fuelHudRoot == null) return;

        Debug.Log("[FuelHUDUnlock] engineUnlocked=" +
    (ProgressionManager.Instance != null ? ProgressionManager.Instance.State.engineUnlocked : false));

        bool unlocked =
            ProgressionManager.Instance != null &&
            ProgressionManager.Instance.State != null &&
            ProgressionManager.Instance.State.engineUnlocked;

        fuelHudRoot.SetActive(unlocked);
    }
}