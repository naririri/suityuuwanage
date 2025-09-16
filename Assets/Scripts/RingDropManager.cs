// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;
// using UnityEngine.SceneManagement; // 追加

// public class RingDropManager : MonoBehaviour
// {
//     public Camera topDownCamera;
//     public Camera mainCamera;
//     public GameObject ringPrefab;
//     public GameObject popupUI;
//     public AudioClip splashSound;
//     public float spawnHeight = 10f;
//     public GameObject ringMarker;

//     public int maxThrows = 5;
//     private int currentThrows = 0;

//     private Vector3 selectedDropPosition;
//     private bool canSelectPosition = true;
//     private bool isDropping = false;
//     private bool waitingForNextThrow = false;
//     private Vector3 topDownInitialPosition;
//     public TextMeshProUGUI throwCountText;
//     public GameObject gameOverPanel; // Inspector でアサイン
//     public GameObject buttonShowResults;
//     public GameObject buttonContinueAd;
//     public GameObject resultPanel;
//     public TextMeshProUGUI resultScoreText;
//     public TextMeshProUGUI resultMessageText;    // メッセージ表示
//     public GameObject crownGold; // Inspectorでアサイン
//     public GameObject crownSilver; // Inspectorでアサイン

//     void Start()
//     {
//         topDownInitialPosition = topDownCamera.transform.position;
//         popupUI.SetActive(false);
//         mainCamera.gameObject.SetActive(false);
//         topDownCamera.gameObject.SetActive(true);
//         ringMarker.SetActive(false);
//         UpdateThrowCountUI(); // 初期表示
//         gameOverPanel.SetActive(false); // ← 最初は非表示
//         buttonShowResults.SetActive(false);
//         buttonContinueAd.SetActive(false);
//         resultPanel.SetActive(false);
//         resultScoreText.gameObject.SetActive(false);
//         resultMessageText.gameObject.SetActive(false);
//     }

//     void Update()
//     {
//         // ✅ マーカーの追従処理（選択中 & 残り投球あり）
//         if (canSelectPosition && !popupUI.activeSelf && currentThrows < maxThrows)
//         {
//             Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
//             if (Physics.Raycast(ray, out RaycastHit hit))
//             {
//                 ringMarker.SetActive(true);
//                 ringMarker.transform.position = hit.point + Vector3.up * 0.1f;
//             }
//         }
//         else
//         {
//             ringMarker.SetActive(false);
//         }

//         // ✅ 落下位置の選択（残り投球がある場合のみ）
//         if (canSelectPosition && !popupUI.activeSelf && Input.GetMouseButtonDown(0) && currentThrows < maxThrows)
//         {
//             Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
//             if (Physics.Raycast(ray, out RaycastHit hit))
//             {
//                 selectedDropPosition = hit.point;
//                 popupUI.SetActive(true);
//             }
//         }

//         // ✅ 次の投球へ戻るクリック
//         if (waitingForNextThrow && Input.GetMouseButtonDown(0))
//         {
//             ReturnToTopView();
//         }
//     }

//     private void UpdateThrowCountUI()
//     {
//         int remaining = maxThrows - currentThrows;
//         throwCountText.text = $"残り：{remaining}回";
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
//         canSelectPosition = true;
//     }

//     private IEnumerator DropSequence()
//     {
//         // ✅ TopDownCameraは動かさず、MainCameraだけ切り替え
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

//         // ✅ カウント加算
//         currentThrows++;
//         // DropSequence() 内の currentThrows++ の直後に追加
//         UpdateThrowCountUI();

//         // // ✅ 最大回数に達したら動作終了（TopDownに戻さない）
//         // if (currentThrows >= maxThrows)
//         // {
//         //     Debug.Log("投球終了！");
//         //     yield break;
//         // }
//         if (currentThrows >= maxThrows)
//         {
//             // Debug.Log("投球終了！");
//             // gameOverPanel.SetActive(true); // ← 表示！
//             // yield break;
//             // 🔸 1〜2秒ほど余韻を持たせてからメッセージを出す
//             yield return new WaitForSeconds(10f);
//             gameOverPanel.SetActive(true);
//             yield return new WaitForSeconds(1f); // アニメ後に自然に登場
//             buttonShowResults.SetActive(true);
//             buttonContinueAd.SetActive(true);
//             yield break;
//         }

//         // ✅ 待機モードでクリック待ち
//         isDropping = false;
//         waitingForNextThrow = true;
//     }
//     public void ShowResults()
//     {
//         //Debug.Log("結果を見るが選ばれました（ここでリザルト画面などに遷移）");
//         Debug.Log("結果画面に遷移します！");

//         gameOverPanel.SetActive(false);
//         resultPanel.SetActive(true);

//         // スコア取得
//         int score = 0;
//         if (ScoreManager.Instance != null)
//         {
//             score = ScoreManager.Instance.totalScore;
//             resultScoreText.text = $"Score：{score}";
//         }
//         else
//         {
//             resultScoreText.text = $"スコアデータなし";
//         }

//         // 👑 すべての王冠を非表示にリセット
//         crownGold.SetActive(false);
//         crownSilver.SetActive(false);

//         // 🎯 得点に応じたメッセージ分岐
//         string message = "";
//         if (score >= 120)
//         {
//             message = "ラッキーな１日になる！";
//             crownGold.SetActive(true);
//         }
//         else if (score >= 80)
//         {
//             message = "良い事あるかも！";
//             crownSilver.SetActive(true);
//         }
//         else if (score >= 40)
//         {
//             message = "Good Job！";
//         }
//         else
//         {
//             message = "前向きに！";
//         }

//         resultMessageText.text = message;

//         // ポップアップとして表示
//         resultScoreText.gameObject.SetActive(true);
//         resultMessageText.gameObject.SetActive(true);
//     }

//     public void ContinueWithAd()
//     {
//         Debug.Log("広告を見て +1 投が選ばれました");
//         maxThrows++;
//         UpdateThrowCountUI();
//         ReturnToTopView();

//         buttonShowResults.SetActive(false);
//         buttonContinueAd.SetActive(false);
//         gameOverPanel.SetActive(false);
//     }

//     private void ReturnToTopView()
//     {
//         // ✅ 必ず初期位置に戻す（ズレを防止）
//         topDownCamera.transform.position = topDownInitialPosition;
//         topDownCamera.gameObject.SetActive(true);
//         mainCamera.gameObject.SetActive(false);

//         canSelectPosition = true;
//         waitingForNextThrow = false;
//     }
//     public void ReturnToTitle()
//     {
//         Debug.Log("[RingDropManager] ReturnToTitle called");
//         SceneManager.LoadScene("Title"); // Titleシーンの名前を指定
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;  // ← 必須

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
    public GameObject gameOverPanel;      // Inspector でアサイン
    public GameObject buttonShowResults;
    public GameObject buttonContinueAd;
    public GameObject resultPanel;
    public GameObject buttonReturnToTitle;   // ← 追加（ResultPanel 配下のボタン）
    public TextMeshProUGUI resultScoreText;
    public TextMeshProUGUI resultMessageText;
    public GameObject crownGold;          // Inspectorでアサイン
    public GameObject crownSilver;        // Inspectorでアサイン

    void Start()
    {
        topDownInitialPosition = topDownCamera.transform.position;
        popupUI.SetActive(false);
        mainCamera.gameObject.SetActive(false);
        topDownCamera.gameObject.SetActive(true);
        ringMarker.SetActive(false);
        UpdateThrowCountUI();
        gameOverPanel.SetActive(false);
        buttonShowResults.SetActive(false);
        buttonContinueAd.SetActive(false);
        resultPanel.SetActive(false);
        resultScoreText.gameObject.SetActive(false);
        resultMessageText.gameObject.SetActive(false);
        if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(false); // ← 追加
    }

    void Update()
    {
        // 落下中なら入力を無効化（簡単にコメントアウトできるようにしておく）
        if (isDropping) return;
        // マーカー追従
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

        // 投下位置の選択
        if (canSelectPosition && !popupUI.activeSelf && Input.GetMouseButtonDown(0) && currentThrows < maxThrows)
        {
            Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                selectedDropPosition = hit.point;
                popupUI.SetActive(true);
            }
        }

        // 次の投球へ
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
        isDropping = true; // ← ここで落下フラグON
        StartCoroutine(DropSequence());
    }

    public void CancelDrop()
    {
        popupUI.SetActive(false);
        canSelectPosition = true;
    }

    private IEnumerator DropSequence()
    {
        topDownCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);

        // リング生成
        Quaternion rotation = Quaternion.Euler(0, 0, 0);
        Vector3 spawnPos = selectedDropPosition + Vector3.up * spawnHeight;
        Instantiate(ringPrefab, spawnPos, rotation);

        // サウンド
        if (SoundManager.Instance != null && SoundManager.Instance.splashSound != null)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.splashSound, spawnPos);
        }

        // 投球カウント
        currentThrows++;
        UpdateThrowCountUI();

        if (currentThrows >= maxThrows)
        {
            yield return new WaitForSeconds(10f);
            gameOverPanel.SetActive(true);
            yield return new WaitForSeconds(1f);
            buttonShowResults.SetActive(true);
            buttonContinueAd.SetActive(true);
            yield break;
        }

        isDropping = false;
        waitingForNextThrow = true;
    }

    public void ShowResults()
    {
        gameOverPanel.SetActive(false);
        resultPanel.SetActive(true);

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

        // 王冠リセット
        crownGold.SetActive(false);
        crownSilver.SetActive(false);

        // メッセージ分岐
        string message = "";
        if (score >= 120)
        {
            message = "ラッキーな１日になる！";
            crownGold.SetActive(true);
        }
        else if (score >= 80)
        {
            message = "良い事あるかも！";
            crownSilver.SetActive(true);
        }
        else if (score >= 40)
        {
            message = "Good Job！";
        }
        else
        {
            message = "前向きに！";
        }

        resultMessageText.text = message;
        resultScoreText.gameObject.SetActive(true);
        resultMessageText.gameObject.SetActive(true);
        if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(true);
    }

    public void ContinueWithAd()
    {
        maxThrows++;
        UpdateThrowCountUI();
        ReturnToTopView();

        buttonShowResults.SetActive(false);
        buttonContinueAd.SetActive(false);
        gameOverPanel.SetActive(false);
        if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(false); // ← 追加
    }

    private void ReturnToTopView()
    {
        topDownCamera.transform.position = topDownInitialPosition;
        topDownCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);

        canSelectPosition = true;
        waitingForNextThrow = false;
    }

    // ★ここがReturnボタン用メソッド
    public void ReturnToTitle()
    {
        Debug.Log("[RingDropManager] ReturnToTitle called");
        SceneManager.LoadScene("Title"); // タイトルシーンの正確な名前に変更
    }
}
