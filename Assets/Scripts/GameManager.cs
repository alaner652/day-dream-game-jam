using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("遊戲狀態設定")]
    public GameState currentGameState = GameState.Playing;

    [Header("遊戲設定")]
    public bool pauseOnTimeUp = true;
    public bool allowPause = true;

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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 找到其他組件
        gameTimer = FindObjectOfType<GameTimer>();
        player = FindObjectOfType<PlayerManger>();

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
        if (pauseOnTimeUp)
        {
            Time.timeScale = 0f;
        }
        OnGameEnded?.Invoke();
        Debug.Log("遊戲結束！");
    }

    public void Victory()
    {
        ChangeGameState(GameState.Victory);
        Time.timeScale = 0f;
        Debug.Log("勝利！");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Debug.Log("退出遊戲");
        Application.Quit();
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