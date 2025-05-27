using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using TMPro;

public class WaitingRoomUIController : MonoBehaviour
{
    public GameObject hostPanel; // Shows relay code
    public GameObject joinPanel; // Shows input field and join button

    public TextMeshProUGUI relayCodeText;
    public TMP_InputField lobbyCodeInput;
    public Button joinGameButton;

    private bool isHost;

    private async void Start()
    {
        isHost = MultiplayerManager.Instance.IsHost;

        if (isHost)
        {
            hostPanel.SetActive(true);
            joinPanel.SetActive(false);

            string lobbyCode = await MultiplayerManager.Instance.HostGame();
            relayCodeText.text = lobbyCode;
        }
        else
        {
            hostPanel.SetActive(false);
            joinPanel.SetActive(true);

            joinGameButton.onClick.AddListener(OnJoinGameClicked);
        }
    }

    private async void OnJoinGameClicked()
    {
        string code = lobbyCodeInput.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("Please enter lobby code");
            return;
        }

        bool success = await MultiplayerManager.Instance.JoinGame(code);
        if (!success)
        {
            Debug.LogError("Failed to join lobby");
        }
    }
}
