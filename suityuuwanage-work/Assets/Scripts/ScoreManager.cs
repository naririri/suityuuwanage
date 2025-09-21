using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;
    public int totalScore = 0;
    //public Text scoreText;
    public TextMeshProUGUI scoreText;

    void Awake()
    {
        Instance = this;
    }

    public void AddScore(int score)
    {
        totalScore += score;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + totalScore;
    }
}

