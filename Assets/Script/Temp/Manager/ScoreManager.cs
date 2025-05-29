using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int score = 0;
    public int Score => score;

    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log($"📊 Local score: {score}");
    }
}
