using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    private Lobby currentLobby;
    private string relayJoinCode;
    private int connectedPlayers = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            // Prevent automatic player spawn
            NetworkManager.Singleton.NetworkConfig.PlayerPrefab = null;

            // Optional: Use Connection Approval
            NetworkManager.Singleton.ConnectionApprovalCallback += ApproveConnection;
        }
    }

    public async Task SignIn()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
        }
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

    public async Task JoinGame(string lobbyCode)
    {
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);
            Debug.Log("✅ Joined Lobby: " + currentLobby.Id);

            if (!currentLobby.Data.ContainsKey("relayCode"))
            {
                Debug.LogError("❌ Relay code not found in lobby data.");
                return;
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
        }
        catch (Exception ex)
        {
            Debug.LogError("❌ JoinGame failed: " + ex.Message);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"✅ Client connected: {clientId}");
        connectedPlayers++;

        if (NetworkManager.Singleton.IsHost && connectedPlayers >= 2)
        {
            Debug.Log("✅ Enough players. Loading 'Gameplay' scene...");
            NetworkManager.Singleton.SceneManager.LoadScene("Gameplay", LoadSceneMode.Single);
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
        response.CreatePlayerObject = false; // We'll spawn manually!
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

    public string GetCurrentRelayJoinCode()
    {
        return relayJoinCode;
    }
}
