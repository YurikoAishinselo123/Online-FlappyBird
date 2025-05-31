using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class GameManager : NetworkBehaviour
{
    public float countdownTime = 3f;
    public static GameManager Instance;

    private Dictionary<ulong, int> playerScores = new();
    private bool gameEnded = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartCoroutine(CountdownAndStartGame());
        }
    }

    private IEnumerator CountdownAndStartGame()
    {
        yield return new WaitForSeconds(0.5f); // Allow scene to finish loading

        StartCountdownClientRpc(countdownTime);

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
    private void StartCountdownClientRpc(float countdown)
    {
        if (TimerUI.Instance != null)
        {
            TimerUI.Instance.StartCountdown(countdown);
        }
        else
        {
            Debug.LogWarning("⚠️ TimerUI not found on client!");
        }
    }

    // Called by ScoreManager on each client via ServerRpc
    public void ReceivePlayerScore(ulong clientId, int score)
    {
        if (!IsServer || gameEnded)
            return;

        playerScores[clientId] = score;

        Debug.Log($"📥 Score received from Client {clientId}: {score}");

        // All players reported?
        if (playerScores.Count >= NetworkManager.Singleton.ConnectedClientsIds.Count)
        {
            gameEnded = true;

            int score1 = playerScores.ContainsKey(0) ? playerScores[0] : 0;
            int score2 = playerScores.ContainsKey(1) ? playerScores[1] : 0;

            ulong winnerId = 9999; // default = tie
            if (score1 > score2) winnerId = 0;
            else if (score2 > score1) winnerId = 1;

            ShowGameOverClientRpc(score1, score2, winnerId);
        }
    }

    [ClientRpc]
    private void ShowGameOverClientRpc(int score1, int score2, ulong winnerId)
    {
        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.ShowResults(score1, score2, winnerId);
        }
        else
        {
            Debug.LogWarning("❗ GameOverUI not found!");
        }
    }
}
