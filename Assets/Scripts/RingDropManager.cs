// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class RingDropManager : MonoBehaviour
// {
//     public Camera topDownCamera;          // 上から見るカメラ（位置決定用）
//     public Camera mainCamera;             // 通常プレイ視点カメラ
//     public GameObject ringPrefab;         // 落とすリングのプレハブ
//     public GameObject popupUI;            // YES/NOポップアップUI
//     public AudioClip splashSound;         // 着水音
//     public float spawnHeight = 10f;       // 上から落とす高さ
//     public int maxThrows = 5;           // 🎯 最大投球数（初期値）
//     private int currentThrows = 0;      // 現在の投球数（0からカウント）
//     private Vector3 selectedDropPosition; // プレイヤーが選んだ位置
//     private bool canSelectPosition = true;      // 位置選択モードかどうか
//     private bool isDropping = false;            // 現在ドロップ中かどうか
//     private bool waitingForNextThrow = false;   // 次の投球待機状態かどうか
//     private Vector3 topDownInitialPosition;     //初期位置
//     public GameObject ringMarker; // Inspectorでアサイン

//     void Start()
//     {
//         topDownInitialPosition = topDownCamera.transform.position;
//         popupUI.SetActive(false);
//         mainCamera.gameObject.SetActive(false);
//         topDownCamera.gameObject.SetActive(true);
//         currentThrows = 0; // 初期化
//     }

//     void Update()
//     {   
//         // マーカーの追従処理（位置選択中のみ）
//         if (canSelectPosition && !popupUI.activeSelf)
//         {
//             Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
//             if (Physics.Raycast(ray, out RaycastHit hit))
//             {
//                 ringMarker.SetActive(true); // 表示
//                 ringMarker.transform.position = hit.point + Vector3.up * 0.1f; // 少し浮かせる
//             }
//         }
//         else
//         {
//             ringMarker.SetActive(false); // 選択中以外は非表示
//         }

//         // 🎯 ① 落下位置の選択（通常時のみ）
//         if (canSelectPosition && !popupUI.activeSelf && Input.GetMouseButtonDown(0))
//         {
//             Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
//             if (Physics.Raycast(ray, out RaycastHit hit))
//             {
//                 selectedDropPosition = hit.point;
//                 popupUI.SetActive(true); // YES/NO ポップアップを表示
//             }
//         }


//         // 🎯 ② ドロップ完了後にクリックされたら TopDownCamera に戻す
//         if (waitingForNextThrow && Input.GetMouseButtonDown(0))
//         {
//             ReturnToTopView();
//         }
//     }

//     public void ConfirmDrop()
//     {
//         popupUI.SetActive(false);
//         canSelectPosition = false;
//         isDropping = true;
//         StartCoroutine(DropSequence());
//     }

//     public void CancelDrop()
//     {
//         popupUI.SetActive(false);
//         canSelectPosition = true; // 再選択を許可
//     }

//     private IEnumerator DropSequence()
//     {
//         // ✅ カメラを正面に下げる演出
//         Vector3 start = topDownCamera.transform.position;
//         Vector3 end = mainCamera.transform.position;

//         float t = 0f;
//         while (t < 1f)
//         {
//             t += Time.deltaTime / 1.5f;
//             topDownCamera.transform.position = Vector3.Lerp(start, end, t);
//             yield return null;
//         }

//         // ✅ 正面カメラに切り替え
//         topDownCamera.gameObject.SetActive(false);
//         mainCamera.gameObject.SetActive(true);

//         // ✅ リングを生成
//         Quaternion rotation = Quaternion.Euler(0, 0, 0);
//         Vector3 spawnPos = selectedDropPosition + Vector3.up * spawnHeight;
//         Instantiate(ringPrefab, spawnPos, rotation);

//         // ✅ サウンド再生
//         if (SoundManager.Instance != null && SoundManager.Instance.splashSound != null)
//         {
//             SoundManager.Instance.PlaySound(SoundManager.Instance.splashSound, spawnPos);
//         }

//         // ✅ ドロップ完了。次の投球待ち状態にする
//         isDropping = false;
//         waitingForNextThrow = true;


//         // ✅ 2投目以降に戻す
//         isDropping = false;
//         waitingForNextThrow = true;
//     }
//     private void ReturnToTopView()
//     {
//         topDownCamera.transform.position = topDownInitialPosition; // ✅ Y座標リセット
//         topDownCamera.gameObject.SetActive(true);
//         mainCamera.gameObject.SetActive(false);

//         canSelectPosition = true;
//         waitingForNextThrow = false;
//     }
// }
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RingDropManager : MonoBehaviour
{
    public Camera topDownCamera;
    public Camera mainCamera;
    public GameObject ringPrefab;
    public GameObject popupUI;
    public AudioClip splashSound;
    public float spawnHeight = 10f;
    public GameObject ringMarker;

    public int maxThrows = 5;
    private int currentThrows = 0;

    private Vector3 selectedDropPosition;
    private bool canSelectPosition = true;
    private bool isDropping = false;
    private bool waitingForNextThrow = false;
    private Vector3 topDownInitialPosition;
    public TextMeshProUGUI throwCountText;
    public GameObject gameOverPanel; // Inspector でアサイン
    public GameObject buttonShowResults;
    public GameObject buttonContinueAd;
    public GameObject resultPanel;
    public TextMeshProUGUI resultScoreText;
    public TextMeshProUGUI resultMessageText;    // メッセージ表示
    public GameObject crownGold; // Inspectorでアサイン
    public GameObject crownSilver; // Inspectorでアサイン

    void Start()
    {
        topDownInitialPosition = topDownCamera.transform.position;
        popupUI.SetActive(false);
        mainCamera.gameObject.SetActive(false);
        topDownCamera.gameObject.SetActive(true);
        ringMarker.SetActive(false);
        UpdateThrowCountUI(); // 初期表示
        gameOverPanel.SetActive(false); // ← 最初は非表示
        buttonShowResults.SetActive(false);
        buttonContinueAd.SetActive(false);
        resultPanel.SetActive(false);
        resultScoreText.gameObject.SetActive(false);
        resultMessageText.gameObject.SetActive(false);
    }

    void Update()
    {
        // ✅ マーカーの追従処理（選択中 & 残り投球あり）
        if (canSelectPosition && !popupUI.activeSelf && currentThrows < maxThrows)
        {
            Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                ringMarker.SetActive(true);
                ringMarker.transform.position = hit.point + Vector3.up * 0.1f;
            }
        }
        else
        {
            ringMarker.SetActive(false);
        }

        // ✅ 落下位置の選択（残り投球がある場合のみ）
        if (canSelectPosition && !popupUI.activeSelf && Input.GetMouseButtonDown(0) && currentThrows < maxThrows)
        {
            Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                selectedDropPosition = hit.point;
                popupUI.SetActive(true);
            }
        }

        // ✅ 次の投球へ戻るクリック
        if (waitingForNextThrow && Input.GetMouseButtonDown(0))
        {
            ReturnToTopView();
        }
    }

        private void UpdateThrowCountUI()
    {
        int remaining = maxThrows - currentThrows;
        throwCountText.text = $"残り：{remaining}回";
    }

    public void ConfirmDrop()
    {
        popupUI.SetActive(false);
        canSelectPosition = false;
        isDropping = true;
        StartCoroutine(DropSequence());
    }

    public void CancelDrop()
    {
        popupUI.SetActive(false);
        canSelectPosition = true;
    }

    private IEnumerator DropSequence()
    {
        // ✅ TopDownCameraは動かさず、MainCameraだけ切り替え
        topDownCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);

        // ✅ リングを生成
        Quaternion rotation = Quaternion.Euler(0, 0, 0);
        Vector3 spawnPos = selectedDropPosition + Vector3.up * spawnHeight;
        Instantiate(ringPrefab, spawnPos, rotation);

        // ✅ サウンド再生
        if (SoundManager.Instance != null && SoundManager.Instance.splashSound != null)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.splashSound, spawnPos);
        }

        // ✅ カウント加算
        currentThrows++;
        // DropSequence() 内の currentThrows++ の直後に追加
        UpdateThrowCountUI();

        // // ✅ 最大回数に達したら動作終了（TopDownに戻さない）
        // if (currentThrows >= maxThrows)
        // {
        //     Debug.Log("投球終了！");
        //     yield break;
        // }
        if (currentThrows >= maxThrows)
        {
            // Debug.Log("投球終了！");
            // gameOverPanel.SetActive(true); // ← 表示！
            // yield break;
            // 🔸 1〜2秒ほど余韻を持たせてからメッセージを出す
            yield return new WaitForSeconds(10f);
            gameOverPanel.SetActive(true);
            yield return new WaitForSeconds(1f); // アニメ後に自然に登場
            buttonShowResults.SetActive(true);
            buttonContinueAd.SetActive(true);
            yield break;
        }

        // ✅ 待機モードでクリック待ち
        isDropping = false;
        waitingForNextThrow = true;
    }
    public void ShowResults()
    {
        //Debug.Log("結果を見るが選ばれました（ここでリザルト画面などに遷移）");
        Debug.Log("結果画面に遷移します！");

        gameOverPanel.SetActive(false);
        resultPanel.SetActive(true);

        // スコア取得
        int score = 0;
        if (ScoreManager.Instance != null)
        {
            score = ScoreManager.Instance.totalScore;
            resultScoreText.text = $"Score：{score}";
        }
        else
        {
            resultScoreText.text = $"スコアデータなし";
        }

        // 👑 すべての王冠を非表示にリセット
        crownGold.SetActive(false);
        crownSilver.SetActive(false);

        // 🎯 得点に応じたメッセージ分岐
        string message = "";
        if (score >= 200)
        {
            message = "ラッキーな１日になる！";
            crownGold.SetActive(true);
        }
        else if (score >= 110)
        {
            message = "良い事あるかも！";
            crownSilver.SetActive(true);
        }
        else if (score >= 60)
        {
            message = "Good Job！";
        }
        else
        {
            message = "前向きに考えよう！";
        }

        resultMessageText.text = message;

        // ポップアップとして表示
        resultScoreText.gameObject.SetActive(true);
        resultMessageText.gameObject.SetActive(true);
    }

    public void ContinueWithAd()
    {
        Debug.Log("広告を見て +1 投が選ばれました");
        maxThrows++;
        UpdateThrowCountUI();
        ReturnToTopView();

        buttonShowResults.SetActive(false);
        buttonContinueAd.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    private void ReturnToTopView()
    {
        // ✅ 必ず初期位置に戻す（ズレを防止）
        topDownCamera.transform.position = topDownInitialPosition;
        topDownCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);

        canSelectPosition = true;
        waitingForNextThrow = false;
    }
}
