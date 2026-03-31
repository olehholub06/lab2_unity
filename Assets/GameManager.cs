using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static event Action OnGameOver;

    private string savePath;

    [Header("Статистика")]
    public int remainingLives = 3;
    public int collectedCoins = 0;
    public int totalCoinsCollected = 0;
    public float levelTime = 0f;
    private bool isGameActive = true; 

    public List<ScoreEntry> highScores = new List<ScoreEntry>();

    [Serializable]
    public struct ScoreEntry { public int coins; public string time; public string date; }

    [Serializable]
    public class SaveData { public int totalCoins; public List<ScoreEntry> scores; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            savePath = Path.Combine(Application.persistentDataPath, "gamesave.json");
            LoadFromFile();

           
            remainingLives = 3;
            levelTime = 0f;
            isGameActive = true;
            Debug.Log("<color=green><b>[GameManager]</b> Гра активована. Життів: 3</color>");
        }
        else { Destroy(gameObject); }
    }

    private void Update()
    {
        if (!isGameActive) return;
        levelTime += Time.deltaTime;
    }

    public void LoseLife()
    {

        if (!isGameActive)
        {
            Debug.LogWarning("<b>[GameManager]</b> Спроба зняти життя відхилена: ГРА НЕАКТИВНА");
            return;
        }

        remainingLives--;
        Debug.Log($"<color=red><b>[GameManager]</b> ШКОДА ПРИЙНЯТА! Залишилось життів: {remainingLives}</color>");

        if (remainingLives <= 0)
        {
            remainingLives = 0;
            TriggerLoss();
        }
    }

    private void TriggerLoss()
    {
        isGameActive = false;
        SaveRecord();
        OnGameOver?.Invoke();
        SaveToFile();
        Debug.Log("<color=black><b>[GameManager]</b> КІНЕЦЬ ГРИ.</color>");
    }

    public void AddCoin()
    {
        if (!isGameActive) return;
        collectedCoins++;
        totalCoinsCollected++;
        Debug.Log($"💰 Монета! У цьому забігу: {collectedCoins} | Всього: {totalCoinsCollected}");
    }

    public void SaveRecord()
    {
        ScoreEntry entry = new ScoreEntry
        {
            coins = collectedCoins,
            time = string.Format("{0:00}:{1:00}", Mathf.FloorToInt(levelTime / 60), Mathf.FloorToInt(levelTime % 60)),
            date = DateTime.Now.ToString("dd.MM HH:mm")
        };
        highScores.Add(entry);
    }

    public void SaveToFile()
    {
        SaveData data = new SaveData { totalCoins = totalCoinsCollected, scores = highScores };
        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
    }

    public void LoadFromFile()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            totalCoinsCollected = data.totalCoins;
            highScores = data.scores ?? new List<ScoreEntry>();
        }
    }
}