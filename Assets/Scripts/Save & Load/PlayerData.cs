using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public float[] playerPosition;

    public PlayerData (BoatControl boatControl)
    {
        playerPosition = new float[3];
        playerPosition[0] = boatControl.transform.position.x;
        playerPosition[1] = boatControl.transform.position.y;
        playerPosition[2] = boatControl.transform.position.z;
    }
}
