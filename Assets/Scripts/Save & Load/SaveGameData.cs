using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveGameData
{
    public float[] playerPosition;

    public List<QuestStateEntry> questStates;
    public List<ItemCountEntry> itemCounts;
    public BoatUpgradeState boatUpgrades;

    public SaveGameData() { }

    public SaveGameData (BoatControl player)
    {
        playerPosition = new float[3];
        playerPosition[0] = player.transform.position.x;
        playerPosition[1] = player.transform.position.y;
        playerPosition[2] = player.transform.position.z;

        questStates = QuestManager.Instance != null ? QuestManager.Instance.ExportStates() : new List<QuestStateEntry>();
        itemCounts = ItemCollectionRegistry.Instance != null ? ItemCollectionRegistry.Instance.Export() : new List<ItemCountEntry>();

        boatUpgrades = ProgressionManager.Instance != null ? ProgressionManager.Instance.Export() : new BoatUpgradeState();
    }
}
