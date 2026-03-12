using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveSystem
{

    private static string PathSave => Application.persistentDataPath + "/game.savedGame";

    public static void SaveGame(BoatControl player) 
    {
        var formatter = new BinaryFormatter();

        using FileStream stream = new FileStream(PathSave, FileMode.Create);
        SaveGameData data = new SaveGameData(player);
        formatter.Serialize(stream, data);

        Debug.Log($"SAVED -> {PathSave} | pos=({data.playerPosition[0]}, {data.playerPosition[1]}, {data.playerPosition[2]})" +
        $"| quests={(data.questStates != null ? data.questStates.Count : 0)} | items={(data.itemCounts != null ? data.itemCounts.Count : 0)}");
    }

    public static SaveGameData LoadGame()
    {
        if (!File.Exists(PathSave))
        {
            Debug.Log("Save File does not exist: " + PathSave);
            return null;
        }

        var formatter = new BinaryFormatter();
        using FileStream stream = new FileStream(PathSave, FileMode.Open);

        var data = formatter.Deserialize(stream) as SaveGameData;

        if (data == null)
        {
            Debug.LogError($"[SaveSystem] Deserialized data is NULL.");
            return null;
        }
        
        return data;
    }
}
