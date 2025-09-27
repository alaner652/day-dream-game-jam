using TMPro;
using UnityEngine;
using UnityEngine.UI; // �p�G�n�s UI

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton

    public int score = 0;
    public TMP_Text scoreText; // ���V UI Text�]�� TMP_Text�^

    private void Awake()
    {
        // �O�Ұߤ@���
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ���������|����
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
