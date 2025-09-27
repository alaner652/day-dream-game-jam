using NUnit.Framework.Interfaces;
using TMPro;
using UnityEngine;

namespace GameJam
{
    public class CoinCollector : MonoBehaviour
    {
        [Header("金幣收集設定")]
        public int coinValue = 1;
        public AudioClip collectSound;
            private AudioSource audioSource;
        public GameObject collectEffect;
        public TMP_Text coinText;

        [Header("門解鎖設定")]
        [Tooltip("需要收集多少金幣才能解鎖門")]
        public static int coinsRequiredForDoor = 10;
  public static System.Action OnCoinTrigger;

        [Header("金幣動畫")]
        public bool animateBounce = true;
        public float bounceHeight = 0.5f;
        public float bounceSpeed = 2f;

        [Header("消失特效")]
        public float disappearDuration = 0.3f;
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);


       
        private Vector3 startPosition;
        private static int totalCoins = 0;
        private SpriteRenderer spriteRenderer;
        private bool isCollected = false;

        public static System.Action<int> OnCoinCollected;
        public static System.Action OnAllCoinsCollected;

        void Awake()
        {
            // 每次場景載入時都重置金幣（只有第一個金幣物件執行）
            if (FindObjectsByType<CoinCollector>(FindObjectsSortMode.None).Length == 1)
            {
                ResetCoins();
                Debug.Log("場景載入，金幣計數已重置為 0");
            }
        }

        void Start()
        {

            audioSource = GetComponent<AudioSource>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            startPosition = transform.position;

            // 停用Animator防止圖片跑走
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.enabled = false;
            }

            // 如果coinText未分配，嘗試自動尋找
            if (coinText == null)
            {
                coinText = FindFirstObjectByType<TMP_Text>();
                if (coinText == null)
                {
                    Debug.LogWarning("找不到TMP_Text組件，請手動分配coinText");
                }
            }
             if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

            if (coinText != null)
            {
                coinText.text = " : " + totalCoins.ToString() + "/" + coinsRequiredForDoor.ToString();
            }
        }

        void Update()
        {
            // 浮動動畫
            if (animateBounce && !isCollected)
            {
                float newY = startPosition.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            Debug.Log($"金幣碰撞檢測: {other.name}, Tag: {other.tag}");

            if (other.CompareTag("Player") || other.name == "Player" || other.CompareTag("Untagged"))
            {
                Debug.Log("玩家碰到金幣!");
                isCollected = true;

                if (coinText != null)
                {
                     coinText.text = " : " + totalCoins.ToString()   + "/" + coinsRequiredForDoor.ToString();
                }
                else
                {
                    Debug.LogWarning("CoinText 尚未分配!");
                }

                StartCoroutine(DisappearEffect());
            }
        }

        System.Collections.IEnumerator DisappearEffect()
        {
            // 播放收集音效
            if (collectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(collectSound);
            }

            // 生成收集特效
            

            Vector3 originalScale = transform.localScale;
            Color originalColor = spriteRenderer.color;
            float timer = 0f;

            while (timer < disappearDuration)
            {
                timer += Time.deltaTime;
                float progress = timer / disappearDuration;

                // 縮放效果
                float scaleValue = scaleCurve.Evaluate(progress);
                transform.localScale = originalScale * scaleValue;

                // 淡出效果
                float alphaValue = fadeCurve.Evaluate(progress);
                spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alphaValue);

                yield return null;
            }

            // 完成收集
            CollectCoin();
        }

        void CollectCoin()
        {
            totalCoins += coinValue;
            OnCoinTrigger?.Invoke();
            Debug.Log($"收集金幣! 總數: {totalCoins}/{coinsRequiredForDoor}");
             if (coinText != null)
                {
                     coinText.text = " : " + totalCoins.ToString()   + "/" + coinsRequiredForDoor.ToString();
                }
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }
            OnCoinCollected?.Invoke(totalCoins);

            // 檢查是否收集完所有需要的金幣
            if (totalCoins >= coinsRequiredForDoor)
            {
                Debug.Log("所有金幣收集完畢！門已解鎖！");
                OnAllCoinsCollected?.Invoke();
            }

            if (collectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(collectSound);
            }

            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }

        public static int GetTotalCoins()
        {
            return totalCoins;
        }

        public static void ResetCoins()
        {
            totalCoins = 0;

            // 清除事件監聽器，避免重複綁定
            OnCoinCollected = null;
            OnAllCoinsCollected = null;

            Debug.Log($"金幣計數已重置：{totalCoins}/{coinsRequiredForDoor}");
        }

        public static bool HasAllCoins()
        {
            return totalCoins >= coinsRequiredForDoor;
        }

        public static int GetCoinsRequired()
        {
            return coinsRequiredForDoor;
        }

        public static void ForceFullReset()
        {
            totalCoins = 0;
            OnCoinCollected = null;
            OnAllCoinsCollected = null;

            Debug.Log("CoinCollector 強制完全重置完成");
        }
    }
}