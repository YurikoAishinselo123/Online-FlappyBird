using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameplayUIManager : MonoBehaviour
{
    public GameObject gameOverCanvas;
    public GameObject gamePlayCanvas;
    public GameObject gamePauseCanvas;
    public BirdController birdController;
    public TextMeshProUGUI currentScoreText;
    public TextMeshProUGUI lastScoreText;
    public TextMeshProUGUI highScoreText;
    public ScoreManager scoreManager;

    private void Start()
    {
        Time.timeScale = 1f;
        gameOverCanvas.SetActive(false);
        gamePauseCanvas.SetActive(false);
        gamePlayCanvas.SetActive(true);
    }

    private void Update()
    {
        GameOver();
        // UpdateScore();
    }

    private void GameOver()
    {
        // if (birdController.isDead)
        // {
        //     scoreManager.SaveScore();
        //     lastScoreText.text = scoreManager.score.ToString();
        //     UpdateHighScore();
        //     gameOverCanvas.SetActive(true);
        //     Time.timeScale = 0f;
        // }
    }

    // private void UpdateScore()
    // {
    //     currentScoreText.text = scoreManager.score.ToString();
    // }

    // private void UpdateHighScore()
    // {
    //     int highScore = scoreManager.GetHighScore();
    //     highScoreText.text = highScore.ToString();
    // }

    // public void Pause()
    // {
    //     Time.timeScale = 0f;
    //     gamePauseCanvas.SetActive(true);
    // }

    // public void Resume()
    // {
    //     Time.timeScale = 1f;
    //     gamePauseCanvas.SetActive(false);
    // }

    // public void Restart()
    // {
    //     SceneManager.LoadScene("GamePlay");
    // }

    // public void Quit()
    // {
    //     SceneManager.LoadScene("Mainmenu");
    // }
}