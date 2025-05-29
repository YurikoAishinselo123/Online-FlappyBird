using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PauseUI : MonoBehaviour
{
    public GameObject pausePanel;
    public Button pauseButton;
    public Button resumeButton;
    public Button exitButton;

    private void Start()
    {
        pauseButton.onClick.AddListener(OnPauseClicked);
        resumeButton.onClick.AddListener(OnResumeClicked);
        exitButton.onClick.AddListener(OnExitClicked);

        pausePanel.SetActive(false);

        if (NetworkPauseManager.Instance != null)
        {
            pausePanel.SetActive(NetworkPauseManager.Instance.isPaused.Value);
            NetworkPauseManager.Instance.isPaused.OnValueChanged += (prev, now) =>
            {
                pausePanel.SetActive(now);
            };
        }
    }

    private void OnPauseClicked()
    {
        NetworkPauseManager.Instance?.RequestPauseToggle();
    }

    private void OnResumeClicked()
    {
        if (NetworkPauseManager.Instance != null && NetworkPauseManager.Instance.isPaused.Value)
        {
            NetworkPauseManager.Instance.RequestPauseToggle();
        }
    }

    private void OnExitClicked()
    {
        if (NetworkPauseManager.Instance == null)
        {
            Debug.LogError("❌ NetworkPauseManager is not available.");
            return;
        }

        if (NetworkManager.Singleton.IsServer)
        {
            // Host can directly trigger exit
            pausePanel.SetActive(false);
            NetworkPauseManager.Instance.ExitGameClientRpc();
        }
        else
        {
            // Client requests server to trigger exit for all
            pausePanel.SetActive(false);
            NetworkPauseManager.Instance.ExitGameServerRpc();
        }
    }
}
