using UnityEngine;

public class FuelHUDUnlock : MonoBehaviour
{
    [SerializeField] private string questToUnlock = "race_island_1";
    [SerializeField] private GameObject fueldHudRoot;

    void OnEnable()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestsChanged += Refresh;

        Refresh();
    }

    void OnDisable()
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.OnQuestsChanged -= Refresh;
    }

    private void Refresh()
    {
        if (fueldHudRoot == null) return;

        bool unlocked = QuestManager.Instance != null && QuestManager.Instance.IsCompleted(questToUnlock);
        if (fueldHudRoot.activeSelf != unlocked)
            fueldHudRoot.SetActive(unlocked);
    }
}
