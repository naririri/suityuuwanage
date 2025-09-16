// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class TargetScore : MonoBehaviour
// {
//     public int pointValue = 10;
//     [Header("Effect Prefabs")]
//     public GameObject hitEffectPrefab;  // ParticleSystem → GameObject に変更
//     public GameObject scorePopupPrefab;      // スコア表示用Prefab（UIまたは3D）
//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Ring"))
//         {
//             Debug.Log("リングに当たった！");

//             // スコア加算（1回だけ）
//             ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
//             if (scoreManager != null)
//             {
//                 scoreManager.AddScore(pointValue);
//             }

//             // ===== ヒットエフェクト =====
//             if (hitEffectPrefab != null)
//             {
//                 Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
//                 EffectManager.Instance.PlayEffect(hitEffectPrefab, spawnPos, 2f);
//             }

//             if (hitEffectPrefab != null)
//             {
//                 Vector3 spawnPos = transform.position + Vector3.up * 0.5f;

//                 // エフェクト再生（すでにある）
//                 EffectManager.Instance.PlayEffect(hitEffectPrefab, spawnPos, 2f);

//                 // クリティカル音再生（追加）
//                 SoundManager.Instance.PlaySound(SoundManager.Instance.criticalSound, spawnPos);
//             }
//             // ===== スコアポップアップ =====
//             if (scorePopupPrefab != null)
//             {
//                 Vector3 popupPos = transform.position + Vector3.up * 1.0f;
//                 GameObject popup = Instantiate(scorePopupPrefab, popupPos, Quaternion.identity);

//                 // TextMeshProのテキストを更新（UIや3Dどちらにも対応）
//                 var text = popup.GetComponentInChildren<TMPro.TextMeshPro>();
//                 if (text == null)
//                 {
//                     var textUGUI = popup.GetComponentInChildren<TMPro.TextMeshProUGUI>();
//                     if (textUGUI != null) textUGUI.text = $"+{pointValue}";
//                 }
//                 else
//                 {
//                     text.text = $"+{pointValue}";
//                 }
//                 Destroy(popup, 1.5f); // 1.5秒後に削除
//             }    
//         }
//     }
// }

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;

// public class TargetScore : MonoBehaviour
// {
//     [Header("Score Settings")]
//     public int pointValue = 10;                // このターゲットの得点
//     public GameObject scorePopupPrefab;        // スコアポップアップ用のPrefab

//     [Header("Effect & Sound")]
//     public GameObject hitEffectPrefab;         // エフェクトPrefab
//     public AudioClip criticalSound;            // 効果音（SoundManager経由）

//     private void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Ring"))
//         {
//             Debug.Log($"[DEBUG] Trigger entered by: {other.name}");
//             Debug.Log($"リングに当たった！ +{pointValue}点");

//             // ===== スコア加算 =====
//             if (ScoreManager.Instance != null)
//             {
//                 ScoreManager.Instance.AddScore(pointValue);
//             }

//             // ===== エフェクト再生 =====
//             if (hitEffectPrefab != null && EffectManager.Instance != null)
//             {
//                 Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
//                 EffectManager.Instance.PlayEffect(hitEffectPrefab, spawnPos, 2f);
//             }

//             // ===== 効果音再生 =====
//             if (SoundManager.Instance != null && criticalSound != null)
//             {
//                 SoundManager.Instance.PlaySound(criticalSound, transform.position);
//             }

//             // ===== スコアポップアップ表示 =====
//             if (scorePopupPrefab != null)
//             {
//                 Vector3 popupPos = transform.position + Vector3.up * 2.5f;
//                 Debug.Log($"Popup Spawn Position: {popupPos}");
//                 GameObject popup = Instantiate(scorePopupPrefab, popupPos, Quaternion.identity);

//                 // ポップアップのTextMeshProを取得してスコアを反映
//                 TextMeshPro text = popup.GetComponentInChildren<TextMeshPro>();
//                 if (text != null)
//                 {
//                     text.text = $"+{pointValue}";
//                 }

//                 Destroy(popup, 1.5f); // 1.5秒後に自動削除
//             }

//             // ===== リングを削除 =====
//             Destroy(other.gameObject);
//         }
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // ← UI/3DどちらのTMPでも使えるように

// public class TargetScore : MonoBehaviour
// {
//     public int pointValue = 10;

//     [Header("Effect Prefabs")]
//     public GameObject hitEffectPrefab;   // エフェクト用Prefab
//     public GameObject scorePopupPrefab;  // スコア表示用Prefab（UIでも3DでもOK）

//     void Awake()
//     {
//         Debug.Log($"[TargetScore] Awake on {name}");
//     }

//     private void OnTriggerEnter(Collider other)
//     {
//         Debug.Log($"[TargetScore] OnTriggerEnter fired on {name} by {other.name} (tag={other.tag})");

//         if (!other.CompareTag("Ring"))
//         {
//             Debug.Log($"[TargetScore] Other is not Ring, skip. (tag={other.tag})");
//             return;
//         }

//         // ===== スコア加算 =====
//         var scoreManager = FindObjectOfType<ScoreManager>();
//         if (scoreManager == null)
//         {
//             Debug.LogWarning("[TargetScore] ScoreManager not found in scene!");
//         }
//         else
//         {
//             int before = scoreManager.totalScore;
//             scoreManager.AddScore(pointValue);
//             Debug.Log($"[TargetScore] Score added {pointValue}. total: {before} -> {scoreManager.totalScore}");
//         }

//         // ===== ヒットエフェクト =====
//         if (hitEffectPrefab == null)
//         {
//             Debug.LogWarning("[TargetScore] hitEffectPrefab is NULL (no effect will play).");
//         }
//         else
//         {
//             if (EffectManager.Instance == null)
//             {
//                 Debug.LogWarning("[TargetScore] EffectManager.Instance is NULL (effect cannot play).");
//             }
//             else
//             {
//                 Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
//                 Debug.Log($"[TargetScore] Playing hit effect at {spawnPos}");
//                 EffectManager.Instance.PlayEffect(hitEffectPrefab, spawnPos, 2f);
//             }
//         }

//         // ===== スコアポップアップ =====
//         if (scorePopupPrefab == null)
//         {
//             Debug.LogWarning("[TargetScore] scorePopupPrefab is NULL (no popup).");
//         }
//         else
//         {
//             Vector3 popupPos = transform.position + Vector3.up * 1.0f;
//             Debug.Log($"[TargetScore] Instantiating score popup at {popupPos}");
//             var popup = Instantiate(scorePopupPrefab, popupPos, Quaternion.identity);
//             Debug.Log($"[TargetScore] Popup instantiated: {popup.name}");

//             // TextMeshPro(3D) or TextMeshProUGUI(UI) どちらかを探す
//             var text3D = popup.GetComponentInChildren<TextMeshPro>();
//             var textUI = (text3D == null) ? popup.GetComponentInChildren<TextMeshProUGUI>() : null;

//             if (text3D != null)
//             {
//                 text3D.text = $"+{pointValue}";
//                 Debug.Log("[TargetScore] Set popup text on TextMeshPro (3D).");
//             }
//             else if (textUI != null)
//             {
//                 textUI.text = $"+{pointValue}";
//                 Debug.Log("[TargetScore] Set popup text on TextMeshProUGUI (UI).");
//             }
//             else
//             {
//                 Debug.LogWarning("[TargetScore] No TextMeshPro / TextMeshProUGUI found in popup prefab!");
//             }

//             // 自動削除（存在する場合のみ）
//             Destroy(popup, 1.5f);
//         }

//         // ===== 効果音 =====
//         if (SoundManager.Instance == null)
//         {
//             Debug.LogWarning("[TargetScore] SoundManager.Instance is NULL (no sound).");
//         }
//         else if (SoundManager.Instance.criticalSound == null)
//         {
//             Debug.LogWarning("[TargetScore] criticalSound AudioClip is NULL (no sound).");
//         }
//         else
//         {
//             SoundManager.Instance.PlaySound(SoundManager.Instance.criticalSound, transform.position);
//             Debug.Log("[TargetScore] Played critical sound.");
//         }

//         // （必要なら）重複加点防止
//         // Destroy(other.gameObject);
//     }

//     // 衝突判定の切り分け用（誤ってTriggerではなくCollider衝突になっていないか確認）
//     private void OnCollisionEnter(Collision collision)
//     {
//         Debug.Log($"[TargetScore] OnCollisionEnter on {name} with {collision.gameObject.name} (tag={collision.gameObject.tag})");
//     }
// }


public class TargetScore : MonoBehaviour
{
    public int pointValue = 10;
    public GameObject hitEffectPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ring")) return;

        // ← ここがポイント：位置つき加点でScoreManagerに託す
        var hitPos = transform.position + Vector3.up * 1.0f;
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScoreAt(pointValue, hitPos);

        // 演出（分業のままでOK）
        if (hitEffectPrefab != null && EffectManager.Instance != null)
        {
            var fxPos = transform.position + Vector3.up * 0.5f;
            EffectManager.Instance.PlayEffect(hitEffectPrefab, fxPos, 2f);
        }
        if (SoundManager.Instance != null && SoundManager.Instance.criticalSound != null)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.criticalSound, transform.position);
        }

        // 多重ヒット防止など必要に応じて
        // Destroy(other.gameObject);
    }
}
