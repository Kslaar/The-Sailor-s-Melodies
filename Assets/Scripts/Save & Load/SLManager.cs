using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SLManager : MonoBehaviour
{
    public static SLManager Instance;

    [Header("Scene")]
    [SerializeField] string gameScene = "GameScene";

    [Header("Runtime")]
    [SerializeField] BoatControl player;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        transform.SetParent(null, true);
        DontDestroyOnLoad(gameObject);  
    }

    public void NewGame()
    {
        StartCoroutine(LoadSceneOrNot(loadSave: false));
    }
    public void LoadGameFromMenu()
    {
        StartCoroutine(LoadSceneOrNot(loadSave: true));
    }

    public void SavePlayer()
    {
        EnsurePlayer();
        if (player == null)
        {
            Debug.LogError("Player reference is missing in SLManager!");
            return;
        }
        SaveSystem.SaveGame(player);
    }

    public void LoadPlayer()
    {
        EnsurePlayer();
        if (player == null)
        {
            Debug.LogError("Player reference is missing in SLManager!");
            return;
        }
        ApplySavedData();
    }

    private IEnumerator LoadSceneOrNot(bool loadSave)
    {
        yield return SceneManager.LoadSceneAsync(gameScene);

        yield return null;

        EnsurePlayer(forceRefresh: true); // Wir gehen sicher, dass der Spieler existiert
        if (player == null) 
            yield break;

        if (loadSave)
            ApplySavedData();
        else
        {
            player.transform.position = new Vector3(12, 0, 6);
        }
    }

    private void EnsurePlayer(bool forceRefresh = false)
    {
        if (!forceRefresh && player != null) 
            return;
        player = FindFirstObjectByType<BoatControl>();
        if (player == null)
            Debug.LogError("BoatControl (Player) not found. Exisitng in GameScene?");
    }
    private void ApplySavedData()
    {
        SaveGameData data = SaveSystem.LoadGame();
        if (data == null) return;

        // 1. Items importieren
        if (ItemCollectionRegistry.Instance != null)
            ItemCollectionRegistry.Instance.Import(data.itemCounts);

        // 2. Queststates importieren
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.ImportStates(data.questStates);
            QuestManager.Instance.ReRegisterActiveObjectives();
        }

        // 3. Wir setzen den Spieler
        Vector3 position = new Vector3(
        data.playerPosition[0],
        data.playerPosition[1],
        data.playerPosition[2]
        );

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = position;
            rb.rotation = rb.rotation;
        }
        else
        {
            player.transform.position = position;
        }

        Debug.Log("Applied saved position: " + position);
    }
}
