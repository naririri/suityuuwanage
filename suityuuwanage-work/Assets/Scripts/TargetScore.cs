// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;


// public class TargetScore : MonoBehaviour
// {
//     public int pointValue = 10; // ターゲットごとに設定できるスコア

//     public GameObject hitEffect; // ← 型を ParticleSystem → GameObject に変更！

//     void OnTriggerEnter(Collider other)
//     {
//         if (other.CompareTag("Ring"))
//         {
//             Debug.Log("リングに当たった！");

//             if (hitEffect != null)
//             {
//                 Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
//                 GameObject effectObj = Instantiate(hitEffect, spawnPos, Quaternion.identity);
//                 Debug.Log("エフェクトオブジェクト生成！");
                
//                 ParticleSystem ps = effectObj.GetComponent<ParticleSystem>();
//                 if (ps != null)
//                 {
//                     ps.Play();
//                     Debug.Log("エフェクト再生！");
//                 }
//                 else
//                 {
//                     Debug.LogWarning("ParticleSystemが見つかりません！");
//                 }

//                 Destroy(effectObj, 2f);
//             }
//             else
//             {
//                 Debug.LogWarning("hitEffectが設定されていません！");
//             }
//         }
//     }
// }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetScore : MonoBehaviour
{
    public int pointValue = 10;
    public GameObject hitEffectPrefab;  // ParticleSystem → GameObject に変更

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ring"))
        {
            Debug.Log("リングに当たった！");

            // スコア加算（1回だけ）
            ScoreManager scoreManager = FindObjectOfType<ScoreManager>();
            if (scoreManager != null)
            {
                scoreManager.AddScore(pointValue);
            }


            if (hitEffectPrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
                EffectManager.Instance.PlayEffect(hitEffectPrefab, spawnPos, 2f);
            }
            
            if (hitEffectPrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * 0.5f;

                // エフェクト再生（すでにある）
                EffectManager.Instance.PlayEffect(hitEffectPrefab, spawnPos, 2f);

                // クリティカル音再生（追加）
                SoundManager.Instance.PlaySound(SoundManager.Instance.criticalSound, spawnPos);
            }

            // エフェクト再生
            // if (hitEffectPrefab != null)
            // {
            //     Vector3 spawnPos = transform.position + Vector3.up * 0.5f;
            //     GameObject effectObj = Instantiate(hitEffectPrefab, spawnPos, Quaternion.identity);
            //     Destroy(effectObj, 2f);
            // }

            // // 一度当たったリングは削除
            // Destroy(other.gameObject);
        }
    }
}

