using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    private ScoreManager trackedScoreManager;

    void Start()
    {
        StartCoroutine(WaitForLocalScoreManager());
    }

    private System.Collections.IEnumerator WaitForLocalScoreManager()
    {
        while (trackedScoreManager == null)
        {
            foreach (var netObj in NetworkManager.Singleton.SpawnManager.SpawnedObjectsList)
            {
                ScoreManager scoreMgr = netObj.GetComponent<ScoreManager>();
                if (scoreMgr != null && scoreMgr.IsOwner)
                {
                    trackedScoreManager = scoreMgr;
                    trackedScoreManager.score.OnValueChanged += UpdateScoreText;
                    UpdateScoreText(0, trackedScoreManager.score.Value);
                    yield break;
                }
            }
            yield return null;
        }
    }

    private void UpdateScoreText(int previousValue, int newValue)
    {
        scoreText.text = "Score: " + newValue;
    }

    private void OnDestroy()
    {
        if (trackedScoreManager != null)
        {
            trackedScoreManager.score.OnValueChanged -= UpdateScoreText;
        }
    }
}
