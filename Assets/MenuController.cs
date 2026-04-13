using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using TMPro; 

public class MenuController : MonoBehaviour
{
    [Header("Посилання на UI")]
    public GameObject menuPanel;
    public TextMeshProUGUI statusText;     
    public TextMeshProUGUI collisionsText; 
    public GameObject recordsPanel;
    public TextMeshProUGUI recordsListText;
    private void OnEnable()
    {
        GameManager.OnGameOver += ShowLossMenu;
        GameManager.OnLevelComplete += ShowVictoryMenu;
    }

    private void OnDisable()
    {
        GameManager.OnGameOver -= ShowLossMenu;
        GameManager.OnLevelComplete -= ShowVictoryMenu;
    }

    private void Start()
    {
        if (menuPanel != null)
            menuPanel.SetActive(false);
        if (recordsPanel != null)
            recordsPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
    public void OpenRecords()
    {
        if (GameManager.Instance == null) return;
        recordsListText.text = "<b>ДАТА         МОНЕТИ    ЧАС</b>\n";
        var scores = GameManager.Instance.highScores;
        for (int i = 0; i < scores.Count; i++)
        {
            if (i >= 10) break; 

            var entry = scores[i];
            recordsListText.text += $"{entry.date}    {entry.coins}       {entry.time}\n";
        }

        if (menuPanel != null) menuPanel.SetActive(false);
        if (recordsPanel != null) recordsPanel.SetActive(true);
    }
    public void CloseRecords()
    {
        if (recordsPanel != null) recordsPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    public void ShowLossMenu()
    {
        if (statusText != null) statusText.text = "ГРА ЗАКІНЧЕНА";
        UpdateStats(); 
        ShowMenu();
    }

    public void ShowVictoryMenu()
    {
        if (statusText != null) statusText.text = "РІВЕНЬ ПРОЙДЕНО!";
        UpdateStats(); 
        ShowMenu();
    }

    private void UpdateStats()
    {
        if (collisionsText != null && GameManager.Instance != null)
        {
            
            int count = 3 - GameManager.Instance.remainingLives;
            collisionsText.text = "Зіткнень: " + count;
        }
    }

    public void ShowMenu()
    {
        if (menuPanel != null) menuPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        
    }

    public void RestartGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResetSession();

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.SaveToFile();

        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }


    void Update()
    {
        if (menuPanel == null) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (menuPanel.activeSelf)
            {
                menuPanel.SetActive(false);
                Time.timeScale = 1f;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                if (statusText != null) statusText.text = "ПАУЗА";
                ShowMenu();
            }
        }
    }
}