using UnityEngine;

namespace GameJam
{
    public class SimpleCoinUI : MonoBehaviour
    {
        private int coinCount = 0;

        void Start()
        {
            CoinCollector.OnCoinCollected += UpdateCoinCount;
        }

        void OnDestroy()
        {
            CoinCollector.OnCoinCollected -= UpdateCoinCount;
        }

        void UpdateCoinCount(int newCount)
        {
            coinCount = newCount;
            Debug.Log($"UI更新: 金幣數量 = {coinCount}");
        }

        void OnGUI()
        {
            GUI.Label(new Rect(10, 10, 200, 30), $"金幣: {coinCount}", GUI.skin.box);
        }
    }
}