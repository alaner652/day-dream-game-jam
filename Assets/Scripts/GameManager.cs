using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("遊戲狀態設定")]
    public GameState currentGameState = GameState.Playing;
    public Canvas DoneCanvas;
    public TMP_Text doneText;
    


    public Button restartButton;

    [Header("遊戲設定")]
    public bool pauseOnTimeUp = true;
    public bool allowPause = true;
    public bool autoRestartOnDeath = true;
    [Tooltip("死亡後自動重新開始的延遲時間")]
    public float deathRestartDelay = 1f;

    // 單例模式
    public static GameManager Instance { get; private set; }

    // 事件系統
    public delegate void GameStateChanged(GameState newState);
    public static event GameStateChanged OnGameStateChanged;

    public delegate void GameEvent();
    public static event GameEvent OnGameStarted;
    public static event GameEvent OnGamePaused;
    public static event GameEvent OnGameResumed;
    public static event GameEvent OnGameEnded;

    private GameTimer gameTimer;
    private PlayerManger player;

    

    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    void Awake()
    {
        // 單例模式設定
        if (Instance == null)
        {
            Instance = this;
            // 注意：重新開始時不要保留 GameManager，讓它重新初始化
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 找到其他組件
        gameTimer = FindFirstObjectByType<GameTimer>();
        player = FindFirstObjectByType<PlayerManger>();

        // 確保遊戲狀態正確初始化
        currentGameState = GameState.Playing;
        Time.timeScale = 1f;

        // 開始遊戲
        StartGame();
    }

    void Update()
    {
        HandleInput();
        CheckGameConditions();
    }

    void HandleInput()
    {
        // ESC 鍵暫停/恢復遊戲
        if (Input.GetKeyDown(KeyCode.Escape) && allowPause)
        {
            if (currentGameState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentGameState == GameState.Paused)
            {
                ResumeGame();
            }
        }

        // R 鍵完全重置遊戲
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("按下 R 鍵，執行完全重置");
            FullReset();
        }
    }

    void CheckGameConditions()
    {
        // 檢查計時器是否結束
        if (gameTimer != null && gameTimer.IsGameEnded() && currentGameState == GameState.Playing)
        {
            EndGame();
        }
    }

    public void StartGame()
    {
        ChangeGameState(GameState.Playing);
        Time.timeScale = 1f;
        OnGameStarted?.Invoke();
        Debug.Log("遊戲開始！");
    }

    public void PauseGame()
    {
        if (currentGameState == GameState.Playing)
        {
            ChangeGameState(GameState.Paused);
            Time.timeScale = 0f;
            OnGamePaused?.Invoke();
            Debug.Log("遊戲暫停");
        }
    }
  

    public void ResumeGame()
    {
        if (currentGameState == GameState.Paused)
        {
            ChangeGameState(GameState.Playing);
            Time.timeScale = 1f;
            OnGameResumed?.Invoke();
            Debug.Log("遊戲恢復");
        }
    }

    public void EndGame()
    {
        ChangeGameState(GameState.GameOver);
        OnGameEnded?.Invoke();
        Debug.Log("遊戲結束！");
        DoneCanvas.gameObject.SetActive(true);
        doneText.text = "Game Over!";
        restartButton.onClick.AddListener(RestartGame);
        // 檢查是否自動重新開始
        if (autoRestartOnDeath)
        {
            Debug.Log($"將在 {deathRestartDelay} 秒後自動重新開始...");
            Invoke("RestartGame", deathRestartDelay);
        }
        else if (pauseOnTimeUp)
        {
            Time.timeScale = 0f;
        }
    }

    public void Victory()
    {
        ChangeGameState(GameState.Victory);
        Time.timeScale = 0f;
        Debug.Log("勝利！");
        DoneCanvas.gameObject.SetActive(true);
        doneText.text = "Congraduelations ! You Win!";
        restartButton.onClick.AddListener(RestartGame);
    }

    public void RestartGame()
    {
        Debug.Log("重新載入場景...");
        Time.timeScale = 1f;

        // 取消所有延遲調用
        CancelInvoke();

        // 重新載入當前場景
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("退出遊戲");
        Application.Quit();
    }

    public void FullReset()
    {
        Debug.Log("執行完全重置...");

        // 重置時間
        Time.timeScale = 1f;

        // 清除所有 PlayerPrefs (如果有用到)
        PlayerPrefs.DeleteAll();

        // 強制重置金幣系統
        GameJam.CoinCollector.ResetCoins();

        // 重新載入場景
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

        Debug.Log("完全重置完成");
    }

    void ChangeGameState(GameState newState)
    {
        GameState previousState = currentGameState;
        currentGameState = newState;
        OnGameStateChanged?.Invoke(newState);
        Debug.Log($"遊戲狀態改變: {previousState} → {newState}");
    }

    // 公開方法供其他腳本使用
    public bool IsGamePlaying()
    {
        return currentGameState == GameState.Playing;
    }

    public bool IsGamePaused()
    {
        return currentGameState == GameState.Paused;
    }

    public bool IsGameEnded()
    {
        return currentGameState == GameState.GameOver || currentGameState == GameState.Victory;
    }

    public GameState GetCurrentState()
    {
        return currentGameState;
    }
}