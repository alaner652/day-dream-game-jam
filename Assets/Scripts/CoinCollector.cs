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
        public bool animateRotation = true;
        public float rotationSpeed = 90f;
        public bool animateBounce = true;
        public float bounceHeight = 0.5f;
        public float bounceSpeed = 2f;

        private AudioSource audioSource;
        private Vector3 startPosition;
        private static int totalCoins = 0;

        public static System.Action<int> OnCoinCollected;

        void Start()
        {
            audioSource = GetComponent<AudioSource>();
            startPosition = transform.position;

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
            if (animateRotation)
            {
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
            }

            if (animateBounce)
            {
                float newY = startPosition.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
                transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"金幣碰撞檢測: {other.name}, Tag: {other.tag}");

            if (other.CompareTag("Player") || other.name == "Player" || other.CompareTag("Untagged"))
            {
                Debug.Log("玩家碰到金幣!");
                Debug.Log(coinText);

                if (coinText != null)
                {
                    coinText.text = " : " + (totalCoins + coinValue).ToString();
                }
                else
                {
                    Debug.LogWarning("CoinText 尚未分配!");
                }

                CollectCoin();
            }
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