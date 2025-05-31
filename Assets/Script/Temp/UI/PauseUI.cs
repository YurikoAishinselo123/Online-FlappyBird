using UnityEngine;
using UnityEngine.UI;
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
        pauseButton.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        if (NetworkPauseManager.Instance != null)
        {
            NetworkPauseManager.Instance.isPauseUIVisible.OnValueChanged += OnPauseUIVisibilityChanged;
            OnPauseUIVisibilityChanged(false, NetworkPauseManager.Instance.isPauseUIVisible.Value);
        }
    }

    private void OnDisable()
    {
        if (NetworkPauseManager.Instance != null)
        {
            NetworkPauseManager.Instance.isPauseUIVisible.OnValueChanged -= OnPauseUIVisibilityChanged;
        }
    }

    private void OnPauseUIVisibilityChanged(bool previous, bool current)
    {
        pausePanel.SetActive(current);
        pauseButton.gameObject.SetActive(!current);
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

        pausePanel.SetActive(false);
        pauseButton.gameObject.SetActive(true);

        if (NetworkManager.Singleton.IsServer)
        {
            NetworkPauseManager.Instance.ExitGameClientRpc();
        }
        else
        {
            NetworkPauseManager.Instance.ExitGameServerRpc();
        }
    }
}
