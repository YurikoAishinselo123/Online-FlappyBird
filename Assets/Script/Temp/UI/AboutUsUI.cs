using UnityEngine;
using UnityEngine.UI;


public class AboutUsUI : MonoBehaviour
{
    [SerializeField] private Button backButton;

    void Awake()
    {
        backButton.onClick.AddListener(BackGame);
    }

    public void BackGame()
    {
        SceneLoader.Instance.LoadMainMenu();
    }
}