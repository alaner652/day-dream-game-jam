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
        public GameObject collectEffect;
        public TMP_Text coinText;

        [Header("金幣動畫")]
        public bool animateBounce = true;
        public float bounceHeight = 0.5f;
        public float bounceSpeed = 2f;

        [Header("消失特效")]
        public float disappearDuration = 0.3f;
        public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
        public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);


        private AudioSource audioSource;
        private Vector3 startPosition;
        private static int totalCoins = 0;
        private SpriteRenderer spriteRenderer;
        private bool isCollected = false;

        public static System.Action<int> OnCoinCollected;

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
                coinText.text = " : " + totalCoins.ToString();
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
                    coinText.text = " : " + (totalCoins + coinValue).ToString();
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
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }

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
            Debug.Log($"收集金幣! 總數: {totalCoins}");
            
            OnCoinCollected?.Invoke(totalCoins);

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
        }
    }
}