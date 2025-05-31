using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Collections;

public class NetworkPauseManager : NetworkBehaviour
{
    public static NetworkPauseManager Instance { get; private set; }

    public NetworkVariable<bool> isPaused = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isPauseUIVisible = new NetworkVariable<bool>(false); // NEW: controls PauseUI visibility

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        isPaused.OnValueChanged += OnPauseStateChanged;
    }

    private void OnDisable()
    {
        isPaused.OnValueChanged -= OnPauseStateChanged;
    }

    private void OnPauseStateChanged(bool previous, bool current)
    {
        Time.timeScale = current ? 0f : 1f;
        Debug.Log($"⏯ Pause changed: {current}");
    }

    public void RequestPauseToggle()
    {
        if (IsServer)
        {
            if (isPaused.Value)
            {
                // Resume: hide PauseUI immediately, keep pause active during countdown
                isPauseUIVisible.Value = false;
                StartCoroutine(ResumeAfterCountdown(3f));
            }
            else
            {
                // Pause: pause game & show PauseUI
                isPaused.Value = true;
                isPauseUIVisible.Value = true;
            }
        }
        else
        {
            TogglePauseServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TogglePauseServerRpc()
    {
        if (isPaused.Value)
        {
            isPauseUIVisible.Value = false;
            StartCoroutine(ResumeAfterCountdown(3f));
        }
        else
        {
            isPaused.Value = true;
            isPauseUIVisible.Value = true;
        }
    }

    private IEnumerator ResumeAfterCountdown(float countdown)
    {
        ShowResumeCountdownClientRpc(countdown);

        yield return new WaitForSecondsRealtime(countdown); // unaffected by Time.timeScale

        isPaused.Value = false;
    }

    [ClientRpc]
    private void ShowResumeCountdownClientRpc(float time)
    {
        if (TimerUI.Instance != null)
        {
            TimerUI.Instance.StartCountdown(time);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ExitGameServerRpc()
    {
        Debug.Log("📡 ExitGameServerRpc called from client.");
        ExitGameClientRpc();
    }

    [ClientRpc]
    public void ExitGameClientRpc()
    {
        Debug.Log("🔚 Exiting to Main Menu...");
        Time.timeScale = 1f;

        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.LoadMainMenu();
        }
        else
        {
            SceneManager.LoadScene("MainMenu");
        }

        if (IsHost)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
