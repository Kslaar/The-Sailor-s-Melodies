using UnityEngine;

public class SaveLoadUI : MonoBehaviour
{
    public void Save() => SLManager.Instance.SavePlayer();
    public void Load() => SLManager.Instance.LoadPlayer();
}
