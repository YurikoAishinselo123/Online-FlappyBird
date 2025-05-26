using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text hostCodeDisplay;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;

    private async void Start()
    {
        await MultiplayerManager.Instance.SignIn();

        hostButton.onClick.AddListener(async () =>
        {
            string code = await MultiplayerManager.Instance.HostGame();
            hostCodeDisplay.text = "Lobby Code: " + code;
        });

        joinButton.onClick.AddListener(async () =>
        {
            string input = joinCodeInput.text.Trim();
            if (!string.IsNullOrEmpty(input))
                await MultiplayerManager.Instance.JoinGame(input);
            else
                Debug.LogWarning("Join code is empty!");
        });
    }
}
