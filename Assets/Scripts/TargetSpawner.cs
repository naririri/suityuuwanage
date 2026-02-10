using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    public GameObject targetPrefab;
    public int spawnCount = 5;
    public Vector2 spawnAreaMin = new Vector2(-1.5f, -1.5f);// 範囲の最小（X,Z）
    public Vector2 spawnAreaMax = new Vector2(1.5f, 1.5f);// 範囲の最小（X,Z）
    public float minDistanceBetweenTargets = 1f; // ポール同士の最小距離

    private List<Vector3> spawnedPositions = new List<Vector3>(); // すでに配置された位置

    void Start()
    {
        int attempts = 0; // 無限ループ防止用
        int maxAttempts = 100; // 最大試行回数

        for (int i = 0; i < spawnCount; i++)
        {
            bool positionFound = false;

            while (!positionFound && attempts < maxAttempts)
            {
                attempts++;

                Vector3 candidatePos = new Vector3(
                    Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                    0f,
                    Random.Range(spawnAreaMin.y, spawnAreaMax.y)
                );

                // 他のポールと十分に離れているかチェック
                bool isTooClose = false;
                foreach (Vector3 pos in spawnedPositions)
                {
                    if (Vector3.Distance(candidatePos, pos) < minDistanceBetweenTargets)
                    {
                        isTooClose = true;
                        break;
                    }
                }

                if (!isTooClose)
                {
                    // 配置確定！
                    GameObject newTarget = Instantiate(targetPrefab, candidatePos, Quaternion.identity);

                    // スコア設定
                    TargetScore scoreScript = newTarget.GetComponent<TargetScore>();
                    if (scoreScript != null)
                    {
                        scoreScript.pointValue = Random.Range(1, 4) * 10;
                    }

                    spawnedPositions.Add(candidatePos);
                    positionFound = true;
                }
            }
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogWarning("TargetSpawner: 配置に失敗したポールがあります（最大試行回数に到達）");
        }
    }
}