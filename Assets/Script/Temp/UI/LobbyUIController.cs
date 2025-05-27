using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class LobbyUIController : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;

    private async void Start()
    {
        if (MultiplayerManager.Instance == null)
        {
            Debug.LogError("❌ MultiplayerManager.Instance is null. Make sure it's in the scene!");
            return;
        }

        Debug.Log($"Create Button: {createButton}, Join Button: {joinButton}");

        if (createButton == null || joinButton == null)
        {
            Debug.LogError("❌ One or more UI buttons are not assigned. Destroying this instance to prevent crashes.");
            Destroy(this); // Optional safeguard
            return;
        }

        await MultiplayerManager.Instance.SignIn();

        createButton.onClick.AddListener(OnCreateClicked);
        joinButton.onClick.AddListener(OnJoinClicked);
    }



    public async Task InitializeServices()
    {
        try
        {
            await MultiplayerManager.Instance.SignIn();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ Failed to sign in: " + ex.Message);
        }
    }

    public void OnCreateClicked()
    {
        if (MultiplayerManager.Instance != null)
        {
            MultiplayerManager.Instance.SetJoinType(LobbyJoinType.Host);
            SceneLoader.Instance.LoadWaitingRoom();
        }
    }

    public void OnJoinClicked()
    {
        if (MultiplayerManager.Instance != null)
        {
            MultiplayerManager.Instance.SetJoinType(LobbyJoinType.Client);
            SceneLoader.Instance.LoadWaitingRoom();
        }
    }
}
