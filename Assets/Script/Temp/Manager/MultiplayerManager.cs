using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public enum LobbyJoinType { None, Host, Client }

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    private Lobby currentLobby;
    private string relayJoinCode;
    private int connectedPlayers = 0;

    public LobbyJoinType JoinType { get; private set; } = LobbyJoinType.None;
    public bool IsHost => JoinType == LobbyJoinType.Host;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            NetworkManager.Singleton.ConnectionApprovalCallback += ApproveConnection;
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null; // Use manual player spawning
        }
    }

    public async Task SignIn()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("✅ Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
    }

    public void SetJoinType(LobbyJoinType type)
    {
        JoinType = type;
    }

    public async Task<string> HostGame()
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(1);
            relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            var lobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "relayCode", new DataObject(DataObject.VisibilityOptions.Public, relayJoinCode) }
                }
            };

            currentLobby = await LobbyService.Instance.CreateLobbyAsync("FlappyLobby", 2, lobbyOptions);
            Debug.Log("✅ Lobby created with code: " + currentLobby.LobbyCode);

            NetworkManager.Singleton.StartHost();
            connectedPlayers = 1;

            return currentLobby.LobbyCode;
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ HostGame failed: " + ex.Message);
            return null;
        }
    }

    public async Task<bool> JoinGame(string lobbyCode)
    {
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            Debug.Log("✅ Joined Lobby: " + currentLobby.Id);

            if (!currentLobby.Data.ContainsKey("relayCode"))
            {
                Debug.LogError("❌ Relay code not found in lobby data.");
                return false;
            }

            relayJoinCode = currentLobby.Data["relayCode"].Value;

            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );

            NetworkManager.Singleton.StartClient();
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ JoinGame failed: " + ex.Message);
            return false;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"✅ Client connected: {clientId}");
        connectedPlayers++;

        if (IsHost && connectedPlayers >= 2)
        {
            Debug.Log("🎮 Enough players. Loading 'Gameplay' scene...");
            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"❌ Client disconnected: {clientId}");
        connectedPlayers = Mathf.Max(connectedPlayers - 1, 0);
    }

    private void ApproveConnection(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = false;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApproveConnection;
        }
    }

    public string GetLobbyCode() => currentLobby?.LobbyCode;
    public string GetRelayJoinCode() => relayJoinCode;
}
