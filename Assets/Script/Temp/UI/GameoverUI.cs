using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public static GameOverUI Instance;

    [Header("UI References")]
    public Canvas canvas;
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;
    [SerializeField]
    private GameObject gameplayCanvas;

    [Header("Winner Images")]
    public GameObject player1WinnerImage;
    public GameObject player2WinnerImage;
    public GameObject tieImage;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        canvas.enabled = false;
    }

    public void ShowResults(int player1Score, int player2Score, ulong winnerId)
    {
        if (canvas != null)
            canvas.enabled = true;

        gameplayCanvas.SetActive(false);

        player1ScoreText.text = player1Score.ToString();
        player2ScoreText.text = player2Score.ToString();

        // Hide all images first
        player1WinnerImage.SetActive(false);
        player2WinnerImage.SetActive(false);
        if (tieImage != null) tieImage.SetActive(false);

        // Show the correct winner image
        if (player1Score == player2Score)
        {
            if (tieImage != null)
                tieImage.SetActive(true);
        }
        else if (winnerId == 0)
        {
            player1WinnerImage.SetActive(true);
        }
        else if (winnerId == 1)
        {
            player2WinnerImage.SetActive(true);
        }
    }
}
