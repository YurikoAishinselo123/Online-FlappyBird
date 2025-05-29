using UnityEngine;
using TMPro;
using Unity.Netcode;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    private ScoreManager trackedScoreManager;
    private int lastScore = -1;

    private void Start()
    {
        // If ScoreManager is attached to PlayerObject
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            var playerObj = NetworkManager.Singleton.LocalClient?.PlayerObject;
            if (playerObj != null)
            {
                trackedScoreManager = playerObj.GetComponent<ScoreManager>();
            }
        }

        if (trackedScoreManager == null)
        {
            // Fallback: find any ScoreManager
            trackedScoreManager = FindFirstObjectByType<ScoreManager>();
        }
    }

    private void Update()
    {
        if (trackedScoreManager != null && scoreText != null)
        {
            int currentScore = trackedScoreManager.Score;
            if (currentScore != lastScore)
            {
                scoreText.text = currentScore.ToString();
                lastScore = currentScore;
            }
        }
    }
}
