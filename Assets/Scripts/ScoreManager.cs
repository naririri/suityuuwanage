using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// public class ScoreManager : MonoBehaviour
// {
//     public static ScoreManager Instance;
//     public int totalScore = 0;
//     //public Text scoreText;
//     public TextMeshProUGUI scoreText;

//     void Awake()
//     {
//         Instance = this;
//     }

//     public void AddScore(int score)
//     {
//         totalScore += score;
//         UpdateUI();
//     }

//     void UpdateUI()
//     {
//         if (scoreText != null)
//             scoreText.text = "Score: " + totalScore;
//     }
// }

// ScoreManager.cs（抜粋）
// public class ScoreManager : MonoBehaviour
// {
//     public static ScoreManager Instance;
//     public static event Action<int> OnScoreAdded; // ← 追加：加点量を通知

//     public int totalScore = 0;
//     public TextMeshProUGUI scoreText;

//     void Awake()
//     {
//         Instance = this;
//     }

//     public void AddScore(int score)
//     {
//         totalScore += score;
//         UpdateUI();

//         // ここでイベント発火
//         OnScoreAdded?.Invoke(score);
//     }

//     void UpdateUI()
//     {
//         if (scoreText != null)
//             scoreText.text = "Score: " + totalScore;
//     }
// }


public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    // 既存：点数だけの通知（固定位置でのポップ等に使える）
    public static event Action<int> OnScoreAdded;

    // 追加：ワールド座標つき通知（命中地点にポップを出す用）
    public static event Action<int, Vector3> OnScoreAddedAt;

    public int totalScore = 0;
    public TextMeshProUGUI scoreText;

    void Awake()
    {
        Instance = this;
    }

    // 既存そのまま：位置なし加点
    public void AddScore(int score)
    {
        totalScore += score;
        UpdateUI();
        OnScoreAdded?.Invoke(score);
    }

    // 追加：位置あり加点（ここを使う）
    public void AddScoreAt(int score, Vector3 worldPos)
    {
        totalScore += score;
        UpdateUI();
        OnScoreAdded?.Invoke(score);            // 汎用
        OnScoreAddedAt?.Invoke(score, worldPos); // 場所つき
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + totalScore;
    }
}
