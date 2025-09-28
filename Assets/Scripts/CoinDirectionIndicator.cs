using UnityEngine;
using TMPro;
using GameJam;

[RequireComponent(typeof(AudioSource))]
public class CoinDirectionIndicator : MonoBehaviour
{
    [Header("方向指示設定")]
    [Tooltip("顯示方向提示的時間（秒）")]
    public float displayDuration = 3f;
    [Tooltip("方向文字")]
    public TMP_Text directionText;
    [Tooltip("方向箭頭（可選）")]
    public Transform directionArrow;
    public AudioClip AudioClip;
    AudioSource audioSource;
    private float displayTimer = 0f;
    private bool isShowing = false;
    private Transform player;
    private bool isAllCoinsCollected = false;

    void Start()
    {
        // 初始化音效組件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 設定音量
        audioSource.volume = 1.0f;

        // 自動尋找玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            PlayerManger playerManager = FindFirstObjectByType<PlayerManger>();
            if (playerManager != null)
            {
                playerObj = playerManager.gameObject;
            }
        }

        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // 訂閱金幣收集事件
        CoinCollector.OnCoinTrigger += ShowNearestCoinDirection;
        CoinCollector.OnAllCoinsCollected += OnAllCoinsCollected;

        // 初始隱藏
        HideIndicator();
    }

    void OnDestroy()
    {
        // 取消訂閱事件
        CoinCollector.OnCoinTrigger -= ShowNearestCoinDirection;
        CoinCollector.OnAllCoinsCollected -= OnAllCoinsCollected;
    }

    void Update()
    {
        if (isShowing)
        {
            displayTimer -= Time.deltaTime;

            if (displayTimer <= 0f)
            {
                HideIndicator();
            }
            else
            {
                // 持續更新方向（如果還有金幣的話）
                UpdateDirection();
            }
        }
    }

    void ShowNearestCoinDirection()
    {
        if (player == null)
        {
            Debug.LogWarning("CoinDirectionIndicator: Player not found!");
            return;
        }

        // 尋找最近的金幣
        CoinCollector nearestCoin = FindNearestCoin();

        if (nearestCoin != null)
        {
            // 立即顯示方向指示
            isShowing = true;

            if (directionText != null)
            {
                directionText.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("CoinDirectionIndicator: Direction text is null!");
            }

            if (directionArrow != null)
            {
                directionArrow.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("CoinDirectionIndicator: Direction arrow is null!");
            }

            UpdateDirection();

            // 播放音效並設定延遲時間
            float totalDisplayTime = displayDuration;
            if (audioSource != null && AudioClip != null)
            {
                audioSource.Stop();
                audioSource.clip = AudioClip;
                audioSource.Play();

                // UI顯示時間 = 設定時間 + 音效長度
                float audioLength = AudioClip.length;
                totalDisplayTime = displayDuration + audioLength;
            }

            displayTimer = totalDisplayTime;
        }
    }

    void UpdateDirection()
    {
        if (player == null) return;

        // 檢查是否所有金幣都收集完了
        if (isAllCoinsCollected)
        {
            // 指向出口門
            ExitDoor exitDoor = FindExitDoor();
            if (exitDoor != null)
            {
                Vector3 direction = (exitDoor.transform.position - player.position).normalized;
                UpdateDirectionUI(direction, true, Vector3.Distance(player.position, exitDoor.transform.position));
            }
            else
            {
                HideIndicator();
            }
            return;
        }

        // 正常指向最近的金幣
        CoinCollector nearestCoin = FindNearestCoin();

        if (nearestCoin == null)
        {
            HideIndicator();
            return;
        }

        Vector3 coinDirection = (nearestCoin.transform.position - player.position).normalized;
        float distance = Vector3.Distance(player.position, nearestCoin.transform.position);
        UpdateDirectionUI(coinDirection, false, distance);
    }

    void UpdateDirectionUI(Vector3 direction, bool pointingToExit, float distance)
    {
        // 更新文字方向
        if (directionText != null)
        {
            if (pointingToExit)
            {
                string directionString = GetDirectionString(direction);
                directionText.text = $"🚪 Exit {directionString} ({distance:F1}m)";
                directionText.color = Color.green; // 出口用綠色
            }
            else
            {
                string directionString = GetDirectionString(direction);
                directionText.text = $"{directionString} ({distance:F1}m)";
                directionText.color = Color.white; // 金幣用白色
            }
        }

        // 更新箭頭方向
        if (directionArrow != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            directionArrow.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            // 出口箭頭用綠色
            if (directionArrow.TryGetComponent(out SpriteRenderer arrowRenderer))
            {
                arrowRenderer.color = pointingToExit ? Color.green : Color.white;
            }
        }
    }

    CoinCollector FindNearestCoin()
    {
        if (player == null) return null;

        CoinCollector[] coins = FindObjectsByType<CoinCollector>(FindObjectsSortMode.None);
        CoinCollector nearestCoin = null;
        float nearestDistance = float.MaxValue;

        foreach (CoinCollector coin in coins)
        {
            if (coin == null || !coin.gameObject.activeInHierarchy) continue;

            float distance = Vector3.Distance(player.position, coin.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestCoin = coin;
            }
        }

        return nearestCoin;
    }

    void OnAllCoinsCollected()
    {
        isAllCoinsCollected = true;
        Debug.Log("所有金幣收集完畢，現在指向出口！");

        // 立即顯示指向出口的方向
        ShowExitDirection();
    }

    void ShowExitDirection()
    {
        if (player == null) return;

        ExitDoor exitDoor = FindExitDoor();
        if (exitDoor != null)
        {
            // 顯示方向指示
            isShowing = true;

            if (directionText != null)
            {
                directionText.gameObject.SetActive(true);
            }

            if (directionArrow != null)
            {
                directionArrow.gameObject.SetActive(true);
            }

            // 播放音效
            if (audioSource != null && AudioClip != null)
            {
                audioSource.Stop();
                audioSource.clip = AudioClip;
                audioSource.Play();

                // UI顯示時間 = 設定時間 + 音效長度
                float audioLength = AudioClip.length;
                displayTimer = displayDuration + audioLength;
            }
            else
            {
                displayTimer = displayDuration;
            }

            UpdateDirection();
        }
    }

    ExitDoor FindExitDoor()
    {
        return FindFirstObjectByType<ExitDoor>();
    }

    string GetDirectionString(Vector3 direction)
    {
        string horizontal = "";
        string vertical = "";

        // 水平方向
        if (direction.x > 0.3f)
            horizontal = "Right";
        else if (direction.x < -0.3f)
            horizontal = "Left";

        // 垂直方向
        if (direction.y > 0.3f)
            vertical = "Up";
        else if (direction.y < -0.3f)
            vertical = "Down";

        // 組合方向
        if (horizontal != "" && vertical != "")
            return vertical + " " + horizontal;
        else if (horizontal != "")
            return horizontal;
        else if (vertical != "")
            return vertical;
        else
            return "Nearby";
    }

    void HideIndicator()
    {
        isShowing = false;
        displayTimer = 0f;

        if (directionText != null)
        {
            directionText.gameObject.SetActive(false);
        }

        if (directionArrow != null)
        {
            directionArrow.gameObject.SetActive(false);
        }
    }
}