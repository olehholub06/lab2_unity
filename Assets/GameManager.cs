using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public static event Action OnGameOver;
    public static event Action OnLevelComplete;

    private string savePath;
    private const int MaxHighScores = 20; 

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

            ResetSessionVariables();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!isGameActive) return;
        levelTime += Time.deltaTime;
    }

    public void LoseLife()
    {
        remainingLives--;
        Debug.Log($"<color=red><b>[GameManager]</b> Залишилось життів: {remainingLives}</color>");

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
        SaveToFile(); 

        OnGameOver?.Invoke();
        Debug.Log("Кінець гри.");
    }

    public void FinishLevel()
    {
        if (!isGameActive) return;
        isGameActive = false;

        SaveRecord();
        SaveToFile();

        OnLevelComplete?.Invoke();
        Debug.Log("Рівень пройдено!");
    }

    public void AddCoin()
    {
        if (!isGameActive) return;
        collectedCoins++;
        totalCoinsCollected++;
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
        OptimizeHighScores(); 
    }

    private void OptimizeHighScores()
    {
    
        highScores = highScores
            .OrderByDescending(s => s.coins)
            .ThenBy(s => s.time)
            .ToList();

        if (highScores.Count > MaxHighScores)
        {
            highScores.RemoveRange(MaxHighScores, highScores.Count - MaxHighScores);
        }
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

    public void ResetSession()
    {
        ResetSessionVariables();
    }

    private void ResetSessionVariables()
    {
        remainingLives = 3;
        collectedCoins = 0;
        levelTime = 0f;
        isGameActive = true;
    }
}