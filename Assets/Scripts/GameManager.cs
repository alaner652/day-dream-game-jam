using TMPro;
using UnityEngine;
using UnityEngine.UI; // 如果要連 UI

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton

    public int score = 0;
    public TMP_Text scoreText; // 指向 UI Text（或 TMP_Text）

    private void Awake()
    {
        // 保證唯一實例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 切場景不會消失
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddScore(int value)
    {
        score += value;
        UpdateUI();
    }

    public void ResetScore()
    {
        score = 0;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = " : " + score.ToString();
        }
    }
}
