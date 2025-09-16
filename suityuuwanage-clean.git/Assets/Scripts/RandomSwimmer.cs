using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSwimmer : MonoBehaviour
{
    public float moveSpeed = 1f;        // 移動速度
    public float turnSpeed = 30f;       // 向きを変える速さ
    public float directionChangeInterval = 3f; // ランダムに方向転換する間隔（秒）

    public float areaLimitX = 1f;  // X軸の移動範囲（±）
    public float areaLimitZ = 1f;  // Z軸の移動範囲（±）
    private Vector3 targetDirection;

    private float timeSinceLastChange = 0f;

    void Start()
    {
        PickNewDirection();
    }

    void Update()
    {
        // ① Y座標を強制的に制限（最初に！）
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(pos.y, 0.5f, 3f); // 常に海底から浮いた高さに
        transform.position = pos;

        // ② 高さを維持したまま向きを調整（Y方向の傾きを防ぐ）
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(targetDirection.x, 0f, targetDirection.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime / 100f);

        // ③ 前進（ローカルZ軸に沿って移動）
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // ④ 時間経過で方向変更
        timeSinceLastChange += Time.deltaTime;
        if (timeSinceLastChange >= directionChangeInterval)
        {
            PickNewDirection();
            timeSinceLastChange = 0f;
        }

        // ⑤ 範囲制限（XZ）
        if (Mathf.Abs(transform.position.x) > areaLimitX || Mathf.Abs(transform.position.z) > areaLimitZ)
        {
            Vector3 center = Vector3.zero;
            targetDirection = (center - transform.position).normalized;
            timeSinceLastChange = 0f;
        }
    }

    void PickNewDirection()
    {
        // ランダムな方向を生成（XZ平面）
        float x = Random.Range(-1f, 1f);
        //float y = Random.Range(0f, 0.3f); // 0以上で浮く方向だけ
        float z = Random.Range(-1f, 1f);
        targetDirection = new Vector3(x, 0f, z).normalized;
    }
}