using UnityEngine;

public class SLManager : MonoBehaviour
{
    [SerializeField] BoatControl player;
    public void SavePlayer()
    {
        if (player == null)
            Debug.LogError("Player reference is missing in SLManager!");
        SaveSystem.SavePlayer(player);
    }

    public void LoadPlayer()
    {
        if (player == null)
            Debug.LogError("Player reference is missing in SLManager!");

        PlayerData data = SaveSystem.LoadPlayer();
        if (data == null) return;

        Vector3 position;
        position.x = data.playerPosition[0];
        position.y = data.playerPosition[1];
        position.z = data.playerPosition[2];

        player.transform.position = position;
    }
}
