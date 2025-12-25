// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using TMPro;
// using UnityEngine.SceneManagement;  // ← 必須

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
//     public GameObject gameOverPanel;      // Inspector でアサイン
//     public GameObject buttonShowResults;
//     public GameObject buttonContinueAd;
//     public GameObject resultPanel;
//     public GameObject buttonReturnToTitle;   // ← 追加（ResultPanel 配下のボタン）
//     public TextMeshProUGUI resultScoreText;
//     public TextMeshProUGUI resultMessageText;
//     public GameObject crownGold;          // Inspectorでアサイン
//     public GameObject crownSilver;        // Inspectorでアサイン

//     void Start()
//     {
//         topDownInitialPosition = topDownCamera.transform.position;
//         popupUI.SetActive(false);
//         mainCamera.gameObject.SetActive(false);
//         topDownCamera.gameObject.SetActive(true);
//         ringMarker.SetActive(false);
//         UpdateThrowCountUI();
//         gameOverPanel.SetActive(false);
//         buttonShowResults.SetActive(false);
//         buttonContinueAd.SetActive(false);
//         resultPanel.SetActive(false);
//         resultScoreText.gameObject.SetActive(false);
//         resultMessageText.gameObject.SetActive(false);
//         if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(false); // ← 追加
//     }

//     void Update()
//     {
//         // 落下中なら入力を無効化（簡単にコメントアウトできるようにしておく）
//         if (isDropping) return;
//         // マーカー追従
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

//         // 投下位置の選択
//         if (canSelectPosition && !popupUI.activeSelf && Input.GetMouseButtonDown(0) && currentThrows < maxThrows)
//         {
//             Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
//             if (Physics.Raycast(ray, out RaycastHit hit))
//             {
//                 selectedDropPosition = hit.point;
//                 popupUI.SetActive(true);
//             }
//         }

//         // 次の投球へ
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
//         isDropping = true; // ← ここで落下フラグON
//         StartCoroutine(DropSequence());
//     }

//     public void CancelDrop()
//     {
//         popupUI.SetActive(false);
//         canSelectPosition = true;
//     }

//     private IEnumerator DropSequence()
//     {
//         topDownCamera.gameObject.SetActive(false);
//         mainCamera.gameObject.SetActive(true);

//         // リング生成
//         Quaternion rotation = Quaternion.Euler(0, 0, 0);
//         Vector3 spawnPos = selectedDropPosition + Vector3.up * spawnHeight;
//         Instantiate(ringPrefab, spawnPos, rotation);

//         // サウンド
//         if (SoundManager.Instance != null && SoundManager.Instance.splashSound != null)
//         {
//             SoundManager.Instance.PlaySound(SoundManager.Instance.splashSound, spawnPos);
//         }

//         // 投球カウント
//         currentThrows++;
//         UpdateThrowCountUI();

//         if (currentThrows >= maxThrows)
//         {
//             yield return new WaitForSeconds(10f);
//             gameOverPanel.SetActive(true);
//             yield return new WaitForSeconds(1f);
//             buttonShowResults.SetActive(true);
//             buttonContinueAd.SetActive(true);
//             yield break;
//         }

//         isDropping = false;
//         waitingForNextThrow = true;
//     }

//     public void ShowResults()
//     {
//         gameOverPanel.SetActive(false);
//         resultPanel.SetActive(true);

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

//         // 王冠リセット
//         crownGold.SetActive(false);
//         crownSilver.SetActive(false);

//         // メッセージ分岐
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
//         resultScoreText.gameObject.SetActive(true);
//         resultMessageText.gameObject.SetActive(true);
//         if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(true);
//     }

//     public void ContinueWithAd()
//     {
//         maxThrows++;
//         UpdateThrowCountUI();
//         ReturnToTopView();

//         buttonShowResults.SetActive(false);
//         buttonContinueAd.SetActive(false);
//         gameOverPanel.SetActive(false);
//         if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(false); // ← 追加
//     }

//     private void ReturnToTopView()
//     {
//         topDownCamera.transform.position = topDownInitialPosition;
//         topDownCamera.gameObject.SetActive(true);
//         mainCamera.gameObject.SetActive(false);

//         canSelectPosition = true;
//         waitingForNextThrow = false;
//     }

//     // ★ここがReturnボタン用メソッド
//     public void ReturnToTitle()
//     {
//         Debug.Log("[RingDropManager] ReturnToTitle called");
//         SceneManager.LoadScene("Title"); // タイトルシーンの正確な名前に変更
//     }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;  // ← 必須
using UnityEngine.EventSystems;

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

    // タッチ用：最後に当たった位置を覚えておく
    private Vector3 lastHitPoint;
    private bool hasLastHitPoint = false;
    private bool suppressNextTouchEnd = false;   // NO/YESを押した指のEndを一度だけ無視

    public TextMeshProUGUI throwCountText;
    public GameObject gameOverPanel;      // Inspector でアサイン
    public GameObject buttonShowResults;
    public GameObject buttonContinueAd;
    public GameObject resultPanel;
    public GameObject buttonReturnToTitle;   // ResultPanel 配下のボタン
    public TextMeshProUGUI resultScoreText;
    public TextMeshProUGUI resultMessageText;
    public GameObject goldTorus;          // Inspectorでアサイン
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
        if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(false);
    }

    void Update()
    {
        // 落下中なら入力を無効化
        if (isDropping) return;

        // エディタでは常にマウス、実機モバイルではタッチ、それ以外はマウス
#if UNITY_EDITOR
        HandleMouseInput();
#else
        if (Application.isMobilePlatform)
        {
            HandleTouchInput();
        }
        else
        {
            HandleMouseInput();
        }
#endif
    }

    // ========================
    // PC / エディタ用（元の挙動）
    // ========================
    private void HandleMouseInput()
    {   
        // デバッグ：ちゃんとここに入っているか
        Debug.Log("HandleMouseInput running");
        // マーカー追従
        if (canSelectPosition && !popupUI.activeSelf && currentThrows < maxThrows)
        {
            Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("Raycast hit at " + hit.point);
                ringMarker.SetActive(true);
                ringMarker.transform.position = hit.point + Vector3.up * 0.1f;
            }
            // ヒットしなかったフレームでは何もしない＝前の位置にマーカーが残る
        }
        else
        {
            // 選択中止やポップアップ表示中、投球終了時などは非表示
            ringMarker.SetActive(false);
        }

        // 投下位置の選択（クリック）
        if (canSelectPosition && !popupUI.activeSelf && Input.GetMouseButtonDown(0) && currentThrows < maxThrows)
        {
            Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("Click select at " + hit.point);
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

    // ========================
    // スマホ用（指でなぞる → 離した位置でポップアップ）
    // ========================
    // private void HandleTouchInput()
    // {
    //     // マーカー追従＆投下位置決定
    //     if (canSelectPosition && !popupUI.activeSelf && currentThrows < maxThrows)
    //     {
    //         if (Input.touchCount > 0)
    //         {
    //             Touch t = Input.GetTouch(0);

    //             Ray ray = topDownCamera.ScreenPointToRay(t.position);
    //             if (Physics.Raycast(ray, out RaycastHit hit))
    //             {
    //                 // 指の位置にマーカー追従
    //                 ringMarker.SetActive(true);
    //                 ringMarker.transform.position = hit.point + Vector3.up * 0.1f;

    //                 lastHitPoint = hit.point;
    //                 hasLastHitPoint = true;
    //             }

    //             // 指を離した瞬間に「最後にヒットした位置」で決定
    //             if (t.phase == TouchPhase.Ended && hasLastHitPoint)
    //             {
    //                 selectedDropPosition = lastHitPoint;
    //                 popupUI.SetActive(true);
    //             }
    //         }
    //         else
    //         {
    //             ringMarker.SetActive(false);
    //         }
    //     }
    //     else
    //     {
    //         ringMarker.SetActive(false);
    //     }

    //     // 次の投球へ（画面をタップしたら戻る）
    //     if (waitingForNextThrow && Input.touchCount > 0)
    //     {
    //         Touch t = Input.GetTouch(0);
    //         if (t.phase == TouchPhase.Began)
    //         {
    //             ReturnToTopView();
    //         }
    //     }
    // }
    private void HandleTouchInput()
    {
        if (!(canSelectPosition && !popupUI.activeSelf && currentThrows < maxThrows))
        {
            ringMarker.SetActive(false);
            // 次の投球へ（ポップアップ非表示時のみ）
            if (waitingForNextThrow && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
                ReturnToTopView();
            return;
        }

        if (Input.touchCount == 0)
        {
            ringMarker.SetActive(false);
            return;
        }

        Touch t = Input.GetTouch(0);

        // ① UI上のタッチはゲーム処理をしない
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
            return;

        // ② 直前にUIボタンを押していた指のEndedは一回だけ無視
        if (suppressNextTouchEnd && (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled))
        {
            suppressNextTouchEnd = false;
            return;
        }

        // マーカー追随
        Ray ray = topDownCamera.ScreenPointToRay(t.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            ringMarker.SetActive(true);
            ringMarker.transform.position = hit.point + Vector3.up * 0.1f;
            lastHitPoint = hit.point;
            hasLastHitPoint = true;
        }

        // 指を離した位置で決定（UI上は前段でreturnしている）
        if (t.phase == TouchPhase.Ended && hasLastHitPoint)
        {
            selectedDropPosition = lastHitPoint;
            popupUI.SetActive(true);
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
        hasLastHitPoint = false;          // タッチ用キャッシュをリセット
        suppressNextTouchEnd = true;   // ★ 追加
        StartCoroutine(DropSequence());
    }

    public void CancelDrop()
    {
        popupUI.SetActive(false);
        canSelectPosition = true;
        hasLastHitPoint = false;          // キャンセル時もリセット
        suppressNextTouchEnd = true;   // ★ 追加
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
        if  (score >= 180)
        {
            message = "輪投げの達人です！";
            goldTorus.SetActive(true);
        }
        else if (score >= 120)
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
        if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(false);
    }

    private void ReturnToTopView()
    {
        topDownCamera.transform.position = topDownInitialPosition;
        topDownCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);

        canSelectPosition = true;
        waitingForNextThrow = false;
        hasLastHitPoint = false; // 念のためリセット
    }

    // ★ここがReturnボタン用メソッド
    public void ReturnToTitle()
    {
        Debug.Log("[RingDropManager] ReturnToTitle called");
        SceneManager.LoadScene("Title"); // タイトルシーンの正確な名前に変更
    }
}

