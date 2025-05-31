using UnityEngine;
using UnityEngine.UI;


public class MainmenuUI : MonoBehaviour
{

    [SerializeField] private Button startButton;
    [SerializeField] private Button aboutButton;
    [SerializeField] private Button quitButton;

    void Awake()
    {
        startButton.onClick.AddListener(StartGame);
        aboutButton.onClick.AddListener(AboutGame);
        quitButton.onClick.AddListener(QuitGame);
    }


    public void StartGame()
    {
        SceneLoader.Instance.LoadLobby();
    }

    public void AboutGame()
    {
        SceneLoader.Instance.LoadAboutUs();
    }

    public void QuitGame()
    {
        SceneLoader.Instance.QuitGame();
    }
}