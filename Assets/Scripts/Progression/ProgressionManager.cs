using System;
using UnityEngine;

[DefaultExecutionOrder(-932)]
public class ProgressionManager : MonoBehaviour
{
    public static ProgressionManager Instance { get; private set; }
    public BoatUpgradeState State = new BoatUpgradeState();
    public event Action OnProgressionChanged;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);
    }

    public BoatUpgradeState Export() => State;

    public void Import(BoatUpgradeState imported)
    {
        State = imported ?? new BoatUpgradeState();
        State.Clamp();
        OnProgressionChanged?.Invoke();
    }

    public void UnlockEngineAbility()
    {
        if (State.engineUnlocked) return;
        State.engineUnlocked = true;
        OnProgressionChanged?.Invoke();
    }

    public void UnlockWindGustAbility()
    {
        if (State.windGustUnlocked) return;
        State.windGustUnlocked = true;
        OnProgressionChanged?.Invoke();
    }

    public void AddUpgradeTier(UpgradeType type, int amount = 1)
    {
        amount = Mathf.Max(1, amount);

        switch(type)
        {
            case UpgradeType.EngineStartChance: State.engineStartChanceTier += amount; break;
            case UpgradeType.EngineSpeed:       State.engineSpeedTier += amount; break;
            case UpgradeType.EngineFuel:        State.engineFuelTier += amount; break;
            case UpgradeType.BoatBaseSpeed:     State.boatBaseSpeedTier += amount; break;
            case UpgradeType.WindGust:          State.windGustTier += amount; break;
        }

        State.Clamp();
        OnProgressionChanged?.Invoke();
    }

    /////////////////////////////////////////////////////
}
