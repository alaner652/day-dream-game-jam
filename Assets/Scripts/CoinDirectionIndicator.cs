using UnityEngine;
using TMPro;
using GameJam;
using UnityEngine.Audio;

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

    void Start()
    {
        // 初始化音效組件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 自動尋找玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            playerObj = FindFirstObjectByType<PlayerManger>()?.gameObject;
        }

        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // 訂閱金幣收集事件
        CoinCollector.OnCoinTrigger += ShowNearestCoinDirection;

        // 初始隱藏
        HideIndicator();
    }

    void OnDestroy()
    {
        // 取消訂閱事件
        CoinCollector.OnCoinTrigger -= ShowNearestCoinDirection;
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
        if (player == null) return;

        // 播放音效（停止之前的音效並播放新的）
        if (audioSource != null && AudioClip != null)
        {
            audioSource.Stop();
            audioSource.clip = AudioClip;
            audioSource.Play();
        }

        // 尋找最近的金幣
        CoinCollector nearestCoin = FindNearestCoin();
        
        if (nearestCoin != null)
        {
            isShowing = true;
            displayTimer = displayDuration;

            if (directionText != null)
            {
                directionText.gameObject.SetActive(true);
            }

            if (directionArrow != null)
            {
                directionArrow.gameObject.SetActive(true);
            }

            UpdateDirection();
        }
    }

    void UpdateDirection()
    {
        if (player == null) return;

        CoinCollector nearestCoin = FindNearestCoin();

        if (nearestCoin == null)
        {
            HideIndicator();
            return;
        }

        Vector3 direction = (nearestCoin.transform.position - player.position).normalized;

        // 更新文字方向
        if (directionText != null)
        {
            string directionString = GetDirectionString(direction);
            float distance = Vector3.Distance(player.position, nearestCoin.transform.position);
            directionText.text = $"{directionString} ({distance:F1}m)";
        }

        // 更新箭頭方向
        if (directionArrow != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            directionArrow.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
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