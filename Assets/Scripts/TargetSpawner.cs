using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public GameObject targetPrefab; // プレハブをInspectorでセット
    public int spawnCount = 5;      // 生成する数
    public Vector2 spawnAreaMin = new Vector2(-3f, -3f); // 範囲の最小（X,Z）
    public Vector2 spawnAreaMax = new Vector2(3f, 3f);   // 範囲の最大（X,Z）

    void Start()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                0f, // 海底の高さに合わせて調整（例：0f）
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );

            GameObject newTarget = Instantiate(targetPrefab, pos, Quaternion.identity);

            // pointValue をランダムにする例（10, 20, 30点）
            TargetScore scoreScript = newTarget.GetComponent<TargetScore>();
            if (scoreScript != null)
            {
                scoreScript.pointValue = Random.Range(1, 4) * 10;
            }
        }
    }
}
