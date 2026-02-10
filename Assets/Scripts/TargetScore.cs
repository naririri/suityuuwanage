using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // ← UI/3DどちらのTMPでも使えるように

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

        //多重ヒット防止など必要に応じて
        Destroy(other.gameObject);
    }
}
