using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDrift : MonoBehaviour
{
    private Rigidbody rb;
    public float driftStrength = 1.0f;      // 横方向の揺れの強さ
    public float driftFrequency = 1.0f;     // 揺れの速さ
    public float resistance = 0.98f;        // 徐々に止まる感じ（空気抵抗みたいな）
    public float currentInfluence = 1.0f;   // 潮流の影響度（CurrentManager連動）
    private Vector3 initialDirection;
    // 横初速加える（以下から）
    [Header("Initial Push")]
    public float initialLateralImpulseEasy = 0.5f;
    public float initialLateralImpulseNormal = 1.2f;
    public float initialLateralImpulseHard = 2.0f;

    void OnEnable()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        float power = initialLateralImpulseNormal;
        var diff = (CurrentManager.Instance ? CurrentManager.Instance.currentDifficulty : CurrentManager.Difficulty.Normal);
        if (diff == CurrentManager.Difficulty.Easy)  power = initialLateralImpulseEasy;
        if (diff == CurrentManager.Difficulty.Hard)  power = initialLateralImpulseHard;

        Vector2 dir2 = Random.insideUnitCircle.normalized; // 水平ランダム方向
        Vector3 impulse = new Vector3(dir2.x, 0f, dir2.y) * power;
        rb.AddForce(impulse, ForceMode.Impulse);
    }
    //横初速ここまで

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialDirection = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;
    }

    void FixedUpdate()
        {
            if (!rb) return;

            // 自前ゆらぎ（任意。強すぎるなら弱める or 無効化）
            float drift = Mathf.Sin(Time.time * driftFrequency) * driftStrength;
            Vector3 sideForce = initialDirection * drift;
            rb.AddForce(sideForce, ForceMode.Force);

            // 潮流（位置依存・突風込み）
            if (CurrentManager.Instance != null)
            {
                Vector3 currentForce = CurrentManager.Instance.GetCurrentForce(transform.position) * 1.0f;
                rb.AddForce(currentForce, ForceMode.Acceleration);
            }

            // ふわっと落とす：滞空時間を稼ぐと横ズレが増える
            rb.linearVelocity *= resistance;
        }

}
