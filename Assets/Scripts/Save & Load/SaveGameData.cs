using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveGameData
{
    public float[] playerPosition;
    public float[] playerRotation;

    public List<QuestStateEntry> questStates;
    public List<ItemCountEntry> itemCounts;
    public BoatUpgradeState boatUpgrades;

    public List<string> recordedSoundIDs;
    public int selectedHotbarIndex;

    public SaveGameData() { }

    public SaveGameData (BoatControl player)
    {
        playerPosition = new float[3];
        playerPosition[0] = player.transform.position.x;
        playerPosition[1] = player.transform.position.y;
        playerPosition[2] = player.transform.position.z;

        playerRotation = new float[4];
        playerRotation[0] = player.transform.rotation.x;
        playerRotation[1] = player.transform.rotation.y;
        playerRotation[2] = player.transform.rotation.z;
        playerRotation[3] = player.transform.rotation.w;

        questStates = QuestManager.Instance != null ? QuestManager.Instance.ExportStates() : new List<QuestStateEntry>();
        itemCounts = ItemCollectionRegistry.Instance != null ? ItemCollectionRegistry.Instance.Export() : new List<ItemCountEntry>();
        boatUpgrades = ProgressionManager.Instance != null ? ProgressionManager.Instance.Export() : new BoatUpgradeState();

        if (RecordHotbar.Instance != null)
        {
            recordedSoundIDs = RecordHotbar.Instance.ExportIDs();
            selectedHotbarIndex = RecordHotbar.Instance.SelectedIndex;
        }
        else
        {
            recordedSoundIDs = new List<string>();
            selectedHotbarIndex = 0;
        }
    }
}
