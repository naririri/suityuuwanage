using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterDrift : MonoBehaviour
{
    private Rigidbody rb;
    public float driftStrength = 1.0f;      // 横方向の揺れの強さ
    public float driftFrequency = 1.0f;     // 揺れの速さ
    public float resistance = 0.98f;        // 徐々に止まる感じ（空気抵抗みたいな）

    private Vector3 initialDirection;

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
        // 横方向にゆらゆら揺れる
        float drift = Mathf.Sin(Time.time * driftFrequency) * driftStrength;
        Vector3 sideForce = initialDirection * drift;

        rb.AddForce(sideForce, ForceMode.Force);

        // 水中の抵抗を再現（空気抵抗のように速度を減衰）
        rb.linearVelocity *= resistance;
    }
}
