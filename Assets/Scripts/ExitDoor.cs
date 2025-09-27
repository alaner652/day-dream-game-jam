using UnityEngine;
using GameJam;

public class ExitDoor : MonoBehaviour
{
    [Header("門設定")]
    [Tooltip("是否需要鑰匙才能開門")]
    public bool requiresKeys = true;
    [Tooltip("門關閉時的顏色")]
    public Color lockedColor = Color.red;
    [Tooltip("門開啟時的顏色")]
    public Color unlockedColor = Color.green;
    [Tooltip("玩家標籤")]
    public string playerTag = "Player";

    [Header("勝利設定")]
    [Tooltip("通過門時是否觸發勝利")]
    public bool triggerVictory = true;

    private bool isUnlocked = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D doorCollider;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorCollider = GetComponent<Collider2D>();

        // 如果不需要鑰匙，直接解鎖
        if (!requiresKeys)
        {
            UnlockDoor();
        }
        else
        {
            LockDoor();
            // 監聽金幣收集事件
            CoinCollector.OnAllCoinsCollected += OnAllCoinsCollected;
        }
    }

    void OnDestroy()
    {
        // 取消事件監聽
        CoinCollector.OnAllCoinsCollected -= OnAllCoinsCollected;
    }

    void OnAllCoinsCollected()
    {
        UnlockDoor();
    }

    void LockDoor()
    {
        isUnlocked = false;

        // 設定門為關閉狀態
        if (spriteRenderer != null)
        {
            spriteRenderer.color = lockedColor;
        }

        // 啟用碰撞，阻擋玩家
        if (doorCollider != null)
        {
            doorCollider.isTrigger = false;
        }

        Debug.Log("門已鎖定");
    }

    void UnlockDoor()
    {
        isUnlocked = true;

        // 設定門為開啟狀態
        if (spriteRenderer != null)
        {
            spriteRenderer.color = unlockedColor;
        }

        // 保持碰撞器，但標記為已解鎖（這樣無論是 Trigger 還是 Collision 都能正常工作）
        // if (doorCollider != null)
        // {
        //     doorCollider.isTrigger = true;
        // }

        Debug.Log("門已解鎖！");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Trigger檢測: {other.name}, Tag: {other.tag}, 門已解鎖: {isUnlocked}");

        if ((other.CompareTag(playerTag) || other.name.Contains("Player") || other.CompareTag("Untagged")) && isUnlocked)
        {
            Debug.Log("玩家通過門 (Trigger)");
            PlayerPassedThrough();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Collision檢測: {collision.gameObject.name}, Tag: {collision.gameObject.tag}, 門已解鎖: {isUnlocked}");

        if (collision.gameObject.CompareTag(playerTag) || collision.gameObject.name.Contains("Player") || collision.gameObject.CompareTag("Untagged"))
        {
            if (isUnlocked)
            {
                Debug.Log("玩家通過門 (Collision)");
                PlayerPassedThrough();
            }
            else
            {
                // 玩家撞到鎖定的門
                if (requiresKeys)
                {
                    int collected = CoinCollector.GetTotalCoins();
                    int total = CoinCollector.GetCoinsRequired();
                    Debug.Log($"門被鎖定！需要收集所有金幣。({collected}/{total})");
                }
            }
        }
    }

    void PlayerPassedThrough()
    {
        Debug.Log("玩家通過了門！");

        if (triggerVictory)
        {
            // 觸發勝利
            if (GameManager.Instance != null)
            {
                GameManager.Instance.Victory();
            }
            else
            {
                Debug.Log("恭喜通關！");
            }
        }
    }

    // 公開方法
    public bool IsUnlocked()
    {
        return isUnlocked;
    }

    public void ForceUnlock()
    {
        UnlockDoor();
    }

    public void ForceLock()
    {
        LockDoor();
    }
}