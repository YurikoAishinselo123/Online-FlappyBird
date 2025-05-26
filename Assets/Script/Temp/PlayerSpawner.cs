using UnityEngine;
using Unity.Netcode;
using System.Collections;

public class PlayerSpawner : NetworkBehaviour
{
    public GameObject[] playerVariants;
    public Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        Debug.Log("🔁 PlayerSpawner OnNetworkSpawn called.");

        int i = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            Transform spawnPoint = GetAvailableSpawnPoint();

            GameObject prefabToUse = playerVariants[i % playerVariants.Length];
            GameObject player = Instantiate(prefabToUse, spawnPoint.position, spawnPoint.rotation);

            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(client.ClientId);

            // Immediately freeze and hide player before the countdown
            var controller = player.GetComponent<BirdController>();
            controller.PreGameFreezeClientRpc();

            Debug.Log($"✅ Spawned player {client.ClientId} at {spawnPoint.position}");
            i++;
        }

        // Start game after countdown
        StartCoroutine(StartCountdownAndBeginGame());

        NetworkManager.OnClientConnectedCallback += HandleClientConnected;
    }

    private IEnumerator StartCountdownAndBeginGame()
    {
        yield return new WaitForSeconds(3f); // Delay to start game

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                var controller = client.PlayerObject.GetComponent<BirdController>();
                controller.StartGameClientRpc();
            }
        }
    }


    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"📦 New client connected: {clientId}");

        if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject != null)
        {
            Transform spawnPoint = GetAvailableSpawnPoint();
            NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.transform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            Debug.Log($"✅ Repositioned player {clientId}");
            return;
        }

        int index = (int)(clientId % (ulong)playerVariants.Length);
        Transform spawn = GetAvailableSpawnPoint();
        GameObject player = Instantiate(playerVariants[index], spawn.position, spawn.rotation);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    }

    private Transform GetAvailableSpawnPoint()
    {
        int index = Random.Range(0, spawnPoints.Length);
        return spawnPoints[index];
    }

    private void OnDestroy()
    {
        if (IsServer)
            NetworkManager.OnClientConnectedCallback -= HandleClientConnected;
    }
}
