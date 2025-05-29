using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject[] playerVariants;
    public Transform[] spawnPoints;

    private int currentSpawnIndex = 0;
    private Dictionary<ulong, Transform> clientSpawnPoints = new();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Debug.Log("🔁 PlayerSpawner OnNetworkSpawn called.");

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnPlayerForClient(client.ClientId);
        }

        StartCoroutine(StartCountdownAndBeginGame());

        NetworkManager.OnClientConnectedCallback += HandleClientConnected;
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"📦 New client connected: {clientId}");
        SpawnPlayerForClient(clientId);
    }

    private void SpawnPlayerForClient(ulong clientId)
    {
        // Prevent duplicate spawning
        if (clientSpawnPoints.ContainsKey(clientId))
            return;

        if (currentSpawnIndex >= spawnPoints.Length)
        {
            Debug.LogError("❌ Not enough spawn points for players!");
            return;
        }

        Transform spawnPoint = spawnPoints[currentSpawnIndex];
        clientSpawnPoints[clientId] = spawnPoint;
        currentSpawnIndex++;

        int prefabIndex = (int)(clientId % (ulong)playerVariants.Length);
        GameObject prefabToUse = playerVariants[prefabIndex];

        GameObject player = Instantiate(prefabToUse, spawnPoint.position, spawnPoint.rotation);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        // Freeze player before game starts
        var controller = player.GetComponent<BirdController>();
        controller.PreGameFreezeClientRpc();

        Debug.Log($"✅ Spawned player {clientId} at {spawnPoint.position}");
    }

    private IEnumerator StartCountdownAndBeginGame()
    {
        float countdownTime = 3f;

        yield return new WaitForSeconds(0.1f);

        // Trigger countdown on all clients
        StartLocalCountdownClientRpc(countdownTime);

        yield return new WaitForSeconds(countdownTime + 1f); // countdown + "GO!"

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                var controller = client.PlayerObject.GetComponent<BirdController>();
                controller.StartGameClientRpc();
            }
        }
    }

    [ClientRpc]
    private void StartLocalCountdownClientRpc(float time)
    {
        if (TimerUI.Instance != null)
        {
            TimerUI.Instance.StartCountdown(time);
        }
        else
        {
            Debug.LogWarning("⚠️ TimerUI.Instance is null on client.");
        }
    }

    private void OnDestroy()
    {
        if (IsServer)
            NetworkManager.OnClientConnectedCallback -= HandleClientConnected;
    }
}
