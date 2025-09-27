using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [Header("遊戲計時器設定")]
    [Tooltip("遊戲總時間（秒）")]
    public float gameTime = 600f; // 10分鐘 = 600秒

    private float currentGameTime;
    private bool gameEnded = false;

    void Start()
    {
        currentGameTime = gameTime;
    }

    void Update()
    {
        HandleGameTimer();
    }

    void HandleGameTimer()
    {
        if (!gameEnded && currentGameTime > 0)
        {
            currentGameTime -= Time.deltaTime;

            int minutes = Mathf.FloorToInt(currentGameTime / 60f);
            int seconds = Mathf.FloorToInt(currentGameTime % 60f);

            Debug.Log($"遊戲時間: {minutes:00}:{seconds:00}");

            if (currentGameTime <= 0)
            {
                currentGameTime = 0;
                gameEnded = true;
                GameTimeUp();
            }
        }
    }

    void GameTimeUp()
    {
        Debug.Log("時間到！");

        // 通知 GameManager 遊戲結束
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EndGame();
        }
        else
        {
            // 如果沒有 GameManager，就直接暫停
            Time.timeScale = 0f;
        }
    }

    // 公開方法供其他腳本使用
    public float GetRemainingTime()
    {
        return currentGameTime;
    }

    public bool IsGameEnded()
    {
        return gameEnded;
    }

    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentGameTime / 60f);
        int seconds = Mathf.FloorToInt(currentGameTime % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
}