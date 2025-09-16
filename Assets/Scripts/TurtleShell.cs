using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public class TurtleShell : MonoBehaviour
// {
//     public int scoreBonus = 50;
//     public GameObject luckyEffectPrefab; // LUCKY! テキスト or エフェクト
//     private float lastScoredTime = -10f;
//     public float scoreCooldown = 3f;

//    //private bool hasScored = false;

//     private void OnTriggerEnter(Collider other)
//     {
//        if ((other.CompareTag("Player") || other.CompareTag("Ring")))
//         {
//             if (other.transform.position.y > transform.position.y + 0.3f &&
//                 Time.time - lastScoredTime >= scoreCooldown)
//             {
//                 lastScoredTime = Time.time;

//                 // スコア加算
//                 ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
//                 if (scoreManager != null)
//                 {
//                     scoreManager.AddScore(scoreBonus);
//                 }

//                 // LUCKYエフェクト表示
//                 if (EffectManager.Instance != null && luckyEffectPrefab != null)
//                 {
//                     Vector3 spawnPos = transform.position + Vector3.up * 2f;
//                     EffectManager.Instance.PlayEffect(luckyEffectPrefab, spawnPos, 2f);
//                 }

//                 // ✅ リングを削除（そのオブジェクト自身）
//                 if (other.CompareTag("Ring"))
//                 {
//                     Destroy(other.gameObject);
//                 }
//             }
//         }
        //     if ((other.CompareTag("Player") || other.CompareTag("Ring")) && !hasScored)
        // {
        //     // 高さチェック（乗ったかどうか）
        //     if (other.transform.position.y > transform.position.y + 0.3f) // 0.3f は微調整
        //     {
        //         hasScored = true;

        //         // スコア加算
        //         ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
        //         if (scoreManager != null)
        //             scoreManager.AddScore(scoreBonus);

        //         // エフェクト再生
        //         if (EffectManager.Instance != null && luckyEffectPrefab != null)
        //         {
        //             Vector3 spawnPos = gameObject.transform.position + Vector3.up * 2f;
        //             EffectManager.Instance.PlayEffect(luckyEffectPrefab, spawnPos, 2f);
        //         }
        //     }
        // }
public class TurtleShell : MonoBehaviour
{
    public int scoreBonus = 50;
    public GameObject luckyEffectPrefab; // LUCKY! テキスト or エフェクト

    // 追加：効果音（Inspectorで設定）
    public AudioClip luckySound;

    private float lastScoredTime = -10f;
    public float scoreCooldown = 3f;

    private void OnTriggerEnter(Collider other)
    {
        if ((other.CompareTag("Player") || other.CompareTag("Ring")))
        {
            // 上から当たった場合のみ反応、クールダウンあり
            if (other.transform.position.y > transform.position.y + 0.3f &&
                Time.time - lastScoredTime >= scoreCooldown)
            {
                lastScoredTime = Time.time;

                // ===== スコア加算 =====
                if (ScoreManager.Instance != null)
                {
                    ScoreManager.Instance.AddScore(scoreBonus);
                }

                // ===== エフェクト表示 =====
                if (EffectManager.Instance != null && luckyEffectPrefab != null)
                {
                    Vector3 spawnPos = transform.position + Vector3.up * 2f;
                    EffectManager.Instance.PlayEffect(luckyEffectPrefab, spawnPos, 2f);
                }

                // ===== 効果音再生 =====
                if (SoundManager.Instance != null && luckySound != null)
                {
                    Vector3 soundPos = transform.position + Vector3.up * 1f;
                    SoundManager.Instance.PlaySound(luckySound, soundPos);
                }

                // ===== リング削除（リング衝突時のみ） =====
                if (other.CompareTag("Ring"))
                {
                    Destroy(other.gameObject);
                }
            }
        }
    }
}

