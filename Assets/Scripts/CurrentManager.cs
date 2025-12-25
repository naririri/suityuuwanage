// using System.Collections;
// using UnityEngine;

// public class CurrentManager : MonoBehaviour
// {
//     public static CurrentManager Instance;

//     public enum Difficulty { Easy, Normal, Hard }
//     public Difficulty currentDifficulty = Difficulty.Easy;

//     private Vector3 currentDirection = Vector3.zero;
//     private float driftStrength = 1f;

//     void Awake()
//     {
//         Instance = this;
//     }

//     void Start()
//     {
//         //StartCoroutine(ChangeCurrentRoutine());
//         currentDifficulty = GameSettings.SelectedDifficulty;
//         StartCoroutine(ChangeCurrentRoutine());
//     }

//     IEnumerator ChangeCurrentRoutine()
//     {
//         while (true)
//         {
//             // 難易度に応じた設定
//             float interval = 2f; 
//             switch (currentDifficulty)
//             {
//                 case Difficulty.Easy:
//                     driftStrength = Random.Range(1.5f, 2.0f);
//                     interval = 1.5f;
//                     break;
//                 case Difficulty.Normal:
//                     driftStrength = Random.Range(3.5f, 3.5f);
//                     interval = 1f;
//                     break;
//                 case Difficulty.Hard:
//                     driftStrength = Random.Range(5.0f, 5.5f);
//                     interval = 0.5f;
//                     break;
//             }

//             // ランダムな水平ベクトルを決定
//             currentDirection = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)).normalized;

//             yield return new WaitForSeconds(interval);
//         }
//     }

//     // 他のオブジェクトから潮流ベクトルを取得
//     public Vector3 GetCurrentForce()
//     {
//         return currentDirection * driftStrength;
//     }
// }

using System.Collections;
using UnityEngine;

public class CurrentManager : MonoBehaviour
{
    public static CurrentManager Instance;

    public enum Difficulty { Easy, Normal, Hard }
    public Difficulty currentDifficulty = Difficulty.Easy;

    // ベース流（一定時間ロックされる“潮流本体”）
    private Vector3 baseDir = Vector3.right;
    private float baseStrength = 1f;

    // 突風（短時間だけ強くなる）
    [Header("Gusts")]
    public float gustChance = 0.25f;      // 1サイクルあたり突風が起きる確率
    public float gustMultiplier = 2.0f;   // 突風時の強さ倍率
    public float gustDuration = 0.4f;     // 突風の継続秒数

    // パーリンノイズ（空間・時間で相関のある乱れ）
    [Header("Perlin Noise")]
    public float noiseAmp = 0.5f;         // ノイズの強さ
    public float noiseFreq = 0.3f;        // 時間周波数（大きいほど速く揺れる）
    public float noiseSpatialScale = 0.5f;// 位置依存（渦っぽさ）

    // 難易度別のベース設定
    private Vector2 strengthRange = new Vector2(1f, 2f);
    private Vector2 coherenceTimeRange = new Vector2(1.2f, 2.0f); // 方向を固定する秒数レンジ

    private bool gustActive = false;
    private float nextChangeAt = 0f;

    void Awake() { Instance = this; }

    void Start()
    {
        ApplyDifficultyPreset();
        PickNewBaseFlow(); // 最初の方向と強さ
    }

    void Update()
    {
        // 一定時間までは“同じ方向”を維持（コヒーレンス）
        if (Time.time >= nextChangeAt)
        {
            PickNewBaseFlow();

            // ときどき突風を発生（短時間だけ倍率アップ）
            if (Random.value < gustChance)
                StartCoroutine(GustBurst());
        }
    }

    void ApplyDifficultyPreset()
    {
        switch (currentDifficulty = GameSettings.SelectedDifficulty)
        {
            case Difficulty.Easy:
                strengthRange = new Vector2(1.0f, 2.0f);
                coherenceTimeRange = new Vector2(1.5f, 2.5f);
                noiseAmp = 0.3f; noiseFreq = 0.25f;
                gustChance = 0.15f; gustMultiplier = 1.6f; gustDuration = 0.35f;
                break;
            case Difficulty.Normal:
                strengthRange = new Vector2(2.5f, 4.0f);
                coherenceTimeRange = new Vector2(1.5f, 3.0f);
                noiseAmp = 0.6f; noiseFreq = 0.35f;
                gustChance = 0.25f; gustMultiplier = 2.2f; gustDuration = 0.45f;
                break;
            case Difficulty.Hard:
                strengthRange = new Vector2(4.5f, 6.5f);
                coherenceTimeRange = new Vector2(2.0f, 4.0f); // ※長めに固定=ぐんぐん流れる
                noiseAmp = 0.9f; noiseFreq = 0.5f;
                gustChance = 0.35f; gustMultiplier = 2.8f; gustDuration = 0.55f;
                break;
        }
    }

    void PickNewBaseFlow()
    {
        // 新しい水平ベクトルをランダム採用
        baseDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
        baseStrength = Random.Range(strengthRange.x, strengthRange.y);

        // 次に方向を変える時刻を決定（この間は同じ方向）
        float coherence = Random.Range(coherenceTimeRange.x, coherenceTimeRange.y);
        nextChangeAt = Time.time + coherence;
    }

    IEnumerator GustBurst()
    {
        gustActive = true;
        yield return new WaitForSeconds(gustDuration);
        gustActive = false;
    }

    // 位置依存（空間相関）＋時間依存（時間相関）のノイズ
    Vector3 PerlinNoiseVec(Vector3 worldPos, float t)
    {
        float nx = Mathf.PerlinNoise(worldPos.x * noiseSpatialScale, t * noiseFreq) * 2f - 1f;
        float nz = Mathf.PerlinNoise(worldPos.z * noiseSpatialScale, (t + 100f) * noiseFreq) * 2f - 1f;
        Vector3 n = new Vector3(nx, 0f, nz);
        return n.normalized * noiseAmp;
    }

    public Vector3 GetCurrentForce(Vector3 worldPos)
    {
        float mul = gustActive ? gustMultiplier : 1f;
        Vector3 baseFlow = baseDir * baseStrength * mul;
        Vector3 noise = PerlinNoiseVec(worldPos, Time.time);
        return baseFlow + noise; // ベース流 + 相関ノイズ
    }
}
