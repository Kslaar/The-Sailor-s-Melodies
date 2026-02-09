using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuUIBinder : MonoBehaviour
{
    [SerializeField] Button newGameButton;
    [SerializeField] Button loadGameButton;
    [SerializeField] Button quitButton;

    private void Awake()
    {
        // Szenen-Reload, also Listener weg!!
        newGameButton.onClick.RemoveAllListeners();
        loadGameButton.onClick.RemoveAllListeners();
        quitButton.onClick.RemoveAllListeners();

        newGameButton.onClick.AddListener(() => SLManager.Instance.NewGame());
        loadGameButton.onClick.AddListener(() => SLManager.Instance.LoadGameFromMenu());
        quitButton.onClick.AddListener(Application.Quit);
    }
}
