using UnityEngine;
using Unity.Netcode;

public class ScoreManager : NetworkBehaviour
{
    private int score = 0;
    private bool hasReportedScore = false;

    public int Score => score;

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log($"📊 Local Score (Client {NetworkManager.Singleton.LocalClientId}): {score}");
    }

    // Call this when the game ends
    public void ReportFinalScore()
    {
        if (hasReportedScore) return;

        hasReportedScore = true;

        if (IsOwner)
        {
            SubmitScoreServerRpc(score);
        }
        else
        {
            Debug.LogWarning("❌ Cannot report score from non-owner client.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitScoreServerRpc(int finalScore, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        Debug.Log($"📨 Server received score from Client {clientId}: {finalScore}");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReceivePlayerScore(clientId, finalScore);
        }
        else
        {
            Debug.LogError("⚠️ GameManager.Instance is null on server.");
        }
    }
}
