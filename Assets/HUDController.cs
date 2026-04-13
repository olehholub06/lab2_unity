using UnityEngine;
using TMPro; 

public class HUDController : MonoBehaviour
{
    [Header("Посилання на UI")]
    public TextMeshProUGUI timerText; 

    void Update()
    {
        if (GameManager.Instance != null)
        {
            float time = GameManager.Instance.levelTime;
            timerText.text = FormatTime(time);
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}