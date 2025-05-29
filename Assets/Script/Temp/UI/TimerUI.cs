using UnityEngine;
using TMPro;
using System.Collections;

public class TimerUI : MonoBehaviour
{
    public static TimerUI Instance;

    public TextMeshProUGUI countdownText;
    public Canvas timerCanvas;

    private void Awake()
    {
        Instance = this;
        timerCanvas.enabled = false;
    }

    public void StartCountdown(float countdownTime)
    {
        Debug.Log("tes");
        timerCanvas.enabled = true;
        StartCoroutine(CountdownCoroutine(countdownTime));
    }

    private IEnumerator CountdownCoroutine(float time)
    {
        float currentTime = time;

        while (currentTime > 0)
        {
            countdownText.text = Mathf.CeilToInt(currentTime).ToString();
            yield return new WaitForSeconds(1f);
            currentTime--;
        }

        countdownText.text = "GO!";
        yield return new WaitForSeconds(1f);

        timerCanvas.enabled = false;
    }
}
