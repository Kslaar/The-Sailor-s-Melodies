using System;
using UnityEngine;

[Serializable]
public class BoatUpgradeState
{
    // Unlockables
    public bool engineUnlocked;
    public bool windGustUnlocked;

    // Upgrade Tiers
    public int engineStartChanceTier;
    public int engineSpeedTier;
    public int engineFuelTier;
    public int boatBaseSpeedTier;
    public int windGustTier;

    public void Clamp()
    {
        engineStartChanceTier = Mathf.Clamp(engineStartChanceTier, 0, 3);
        engineSpeedTier       = Mathf.Clamp(engineSpeedTier, 0, 3);
        engineFuelTier        = Mathf.Clamp(engineFuelTier, 0, 3);
        boatBaseSpeedTier     = Mathf.Clamp(boatBaseSpeedTier, 0, 3);
        windGustTier          = Mathf.Clamp(windGustTier, 0, 3);
    }
}
