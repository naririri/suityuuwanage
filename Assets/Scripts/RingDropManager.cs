using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;  // ← 必須
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    private bool waitingReward = false;
    private Coroutine continueBtnRoutine;
    private int _skipNextSelectClickFrames = 0;
    // ★広告から戻った直後の入力を捨てる
    [SerializeField] private float inputBlockSecondsAfterAd = 0.5f;
    private float _inputBlockedUntil = 0f;
    // ★広告後の入力を完全にクリアするまで待つ
    private bool _blockSelectUntilFingerReleased = false;
    private bool blockInputUntilFingerUp = false;
    //private bool _rewardGrantedThisAd = false;
    //private bool _rewardEarnedThisAd = false;// earned(報酬取得)だけを示す
    //private bool _rewardClosedThisAd = false; // ★閉じた通知をUpdateで処理する
    private bool rewardEarnedThisAd = false;
    private bool _ignoreNextSelectOnce = false; // ★広告後の “1回だけ” 選択を無視
    private bool _blockSelectThisFrame = false; // 広告直後の1回だけ選択を無効化


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

    private Coroutine _bindCo;

    void OnEnable()
    {
        if (_bindCo != null) StopCoroutine(_bindCo);
        _bindCo = StartCoroutine(BindAdEventsWhenReady());
    }

    void OnDisable()
    {
        if (_bindCo != null)
        {
            StopCoroutine(_bindCo);
            _bindCo = null;
        }

        if (AdManager.Instance != null)
        {
            AdManager.Instance.OnExtraThrowGranted -= OnExtraThrowGranted;
            AdManager.Instance.OnRewardClosed -= OnRewardClosed;
        }
    }

    private IEnumerator BindAdEventsWhenReady()
    {
        while (AdManager.Instance == null) yield return null;

        AdManager.Instance.OnExtraThrowGranted -= OnExtraThrowGranted;
        AdManager.Instance.OnExtraThrowGranted += OnExtraThrowGranted;

        AdManager.Instance.OnRewardClosed -= OnRewardClosed;
        AdManager.Instance.OnRewardClosed += OnRewardClosed;
    }

    // void OnEnable()
    // {
    //     // ★リワード視聴完了（追加投擲OK）を受け取る
    //     if (AdManager.Instance != null)
    //     {
    //         AdManager.Instance.OnExtraThrowGranted += OnExtraThrowGranted;
    //         AdManager.Instance.OnRewardClosed += OnRewardClosed; // ★追加
    //     }
    // }

    // void OnDisable()
    // {
    //     if (AdManager.Instance != null)
    //     {
    //         AdManager.Instance.OnExtraThrowGranted -= OnExtraThrowGranted;
    //         AdManager.Instance.OnRewardClosed -= OnRewardClosed; // ★追加
    //     }
    // }

    

    // private IEnumerator BindAdEventsWhenReady()
    // {
    //     while (AdManager.Instance == null) yield return null;

    //     AdManager.Instance.OnExtraThrowGranted -= OnExtraThrowGranted;
    //     AdManager.Instance.OnExtraThrowGranted += OnExtraThrowGranted;

    //     AdManager.Instance.OnRewardClosed -= OnRewardClosed;
    //     AdManager.Instance.OnRewardClosed += OnRewardClosed;
    // }

    void Update()
    {
    #if !UNITY_EDITOR
        if (blockInputUntilFingerUp)
        {
            if (Input.touchCount == 0) blockInputUntilFingerUp = false;
            else return;
        }
    #endif

        // ★広告直後の余波を1フレーム吸収（Confirm再発防止）
        if (_blockSelectThisFrame)
        {
            _blockSelectThisFrame = false;
            return;
        }

        // ★広告直後は入力を捨てる（Time.timeScaleの影響を受けない）
        if (Time.unscaledTime < _inputBlockedUntil) return;

        // 落下中なら入力を無効化
        if (isDropping) return;

        // ★広告直後は「指が完全に離れるまで」落下位置選択を禁止
        if (_blockSelectUntilFingerReleased)
        {
            bool noTouch = Input.touchCount == 0;
            bool noMouse = !Input.GetMouseButton(0);

            if (noTouch && noMouse)
            {
                // 指が離れたので解除
                _blockSelectUntilFingerReleased = false;
            }
            else
            {
                // 解除されるまで何もしない（誤タップ吸収）
                return;
            }
        }

        // 念のため（上でreturnしてるので重複だけど残してOK）
        if (isDropping) return;

        // エディタでは常にマウス、実機モバイルではタッチ、それ以外はマウス
    #if UNITY_EDITOR
        HandleMouseInput();
    #else
        if (Application.isMobilePlatform)
            HandleTouchInput();
        else
            HandleMouseInput();
    #endif
    }

    // void Update()
    // {
    // #if !UNITY_EDITOR
    //     if (blockInputUntilFingerUp)
    //     {
    //         if (Input.touchCount == 0) blockInputUntilFingerUp = false;
    //         else return;
    //     }
    // #endif
    //     // ★広告直後は入力を捨てる（Time.timeScaleの影響を受けない）
    //     if (Time.unscaledTime < _inputBlockedUntil) return;
    //     // 落下中なら入力を無効化
    //     if (isDropping) return;
    //     // ★広告直後は「指が完全に離れるまで」落下位置選択を禁止
    //     if (_blockSelectUntilFingerReleased)
    //     {
    //         bool noTouch = Input.touchCount == 0;
    //         bool noMouse = !Input.GetMouseButton(0);

    //         if (noTouch && noMouse)
    //         {
    //             // 指が離れたので解除
    //             _blockSelectUntilFingerReleased = false;
    //         }
    //         else
    //         {
    //         // 解除されるまで何もしない（誤タップ吸収）
    //         return;
    //         }
    //     }

    //     if (isDropping) return;

    //     // エディタでは常にマウス、実機モバイルではタッチ、それ以外はマウス
    //     #if UNITY_EDITOR
    //     HandleMouseInput();
    //     #else
    //     if (Application.isMobilePlatform)
    //     {
    //         HandleTouchInput();
    //     }
    //     else
    //     {
    //         HandleMouseInput();
    //     }
    //     #endif
    //     // ★広告クローズ後の処理はUpdateで必ず走らせる（実機対策）
    //     if (_rewardClosedThisAd)
    //     {
    //         _rewardClosedThisAd = false;

    //         Debug.Log($"[RingDropManager] Process RewardClosed in Update. waitingReward={waitingReward}, earned={_rewardEarnedThisAd}");

    //         // 待機解除
    //         waitingReward = false;

    //         // ★閉じた直後の入力を吸収（PC/モバイル共通）
    //         _inputBlockedUntil = Time.unscaledTime + inputBlockSecondsAfterAd;
    //         suppressNextTouchEnd = true;
    //         _blockSelectUntilFingerReleased = true;
    //         hasLastHitPoint = false;
    //         if (ringMarker != null) ringMarker.SetActive(false);

    //     #if UNITY_EDITOR || UNITY_STANDALONE
    //         _skipNextSelectClickFrames = 10;
    //     #endif
    //         if (EventSystem.current != null)
    //             EventSystem.current.SetSelectedGameObject(null);

    //         if (_rewardEarnedThisAd)
    //         {
    //             _rewardEarnedThisAd = false;

    //             Debug.Log("[RingDropManager] Reward earned -> +1 throw and resume (Update)");

    //             StopContinueAdInteractableRoutine();

    //             // +1回
    //             maxThrows++;
    //             UpdateThrowCountUI();

    //             // UIを閉じる
    //             buttonShowResults.SetActive(false);
    //             buttonContinueAd.SetActive(false);
    //             gameOverPanel.SetActive(false);
    //             if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(false);

    //             // 投げられる状態へ
    //             isDropping = false;
    //             waitingForNextThrow = false;

    //             ReturnToTopView();
    //         }
    //         else
    //         {
    //             Debug.Log("[RingDropManager] Reward closed without grant -> unlock UI (Update)");

    //             // リワードボタンを戻す
    //             if (buttonContinueAd != null)
    //             {
    //                 var btn = buttonContinueAd.GetComponent<Button>();
    //                 if (btn != null) btn.interactable = true;
    //             }
    //         }
    //     }
    // }

    // ========================
    // PC / エディタ用（元の挙動）
    // ========================
    private void HandleMouseInput()
    {   
        if (_skipNextSelectClickFrames > 0)
        {
            _skipNextSelectClickFrames--;
            //return; // ★このフレームはマウス入力を捨てる
        }

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

        // ★広告直後の1回だけ、選択（Confirm表示）を無効化（PC/Editor対策）
        if (_ignoreNextSelectOnce)
        {
            _ignoreNextSelectOnce = false;
            return;
        }

        // 投下位置の選択（クリック）
        // if (canSelectPosition && !popupUI.activeSelf && Input.GetMouseButtonDown(0) && currentThrows < maxThrows)
        // {
        //     Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
        //     if (Physics.Raycast(ray, out RaycastHit hit))
        //     {
        //         Debug.Log("Click select at " + hit.point);
        //         selectedDropPosition = hit.point;
        //         popupUI.SetActive(true);
        //     }
        // }
        if (_skipNextSelectClickFrames == 0 &&
            canSelectPosition && !popupUI.activeSelf &&
            Input.GetMouseButtonDown(0) && currentThrows < maxThrows)
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
        //{
            ReturnToTopView();
        //}
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
            // ★広告直後の1回だけ、選択（Confirm表示）を無効化
            if (_ignoreNextSelectOnce)
            {
                _ignoreNextSelectOnce = false;
                hasLastHitPoint = false;
                return;
            }

            selectedDropPosition = lastHitPoint;
            popupUI.SetActive(true);
        }

        // 次の投球へ
        // if (waitingForNextThrow && Input.GetMouseButtonDown(0))
        // {
        //     // ★広告復帰直後の1回は無視（ReturnToTopViewまで止める）
        //     if (_ignoreNextSelectOnce)
        //     {
        //         _ignoreNextSelectOnce = false;
        // return;
        //     }

        //     ReturnToTopView();
        // }
        // 次の投球へ（スマホ）
        if (waitingForNextThrow && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
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

<<<<<<< HEAD
    // public void ContinueWithAd()
    // {
    //     maxThrows++;
    //     UpdateThrowCountUI();
    //     ReturnToTopView();

    //     buttonShowResults.SetActive(false);
    //     buttonContinueAd.SetActive(false);
    //     gameOverPanel.SetActive(false);
    //     if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(false);
    // }

    // private void ReturnToTopView()
    // {
    //     topDownCamera.transform.position = topDownInitialPosition;
    //     topDownCamera.gameObject.SetActive(true);
    //     mainCamera.gameObject.SetActive(false);

    //     canSelectPosition = true;
    //     waitingForNextThrow = false;
    //     hasLastHitPoint = false; // 念のためリセット
    // }

    // // ★ここがReturnボタン用メソッド
    // public void ReturnToTitle()
    // {
    //     Debug.Log("[RingDropManager] ReturnToTitle called");
    //     SceneManager.LoadScene("Title"); // タイトルシーンの正確な名前に変更
    // }
    // =========================
    // ★ContinueAdボタン用：広告を見る → 視聴完了で +1 → 再開
    // =========================
    public void WatchRewardAndContinue()
    {
        Debug.Log("[RingDropManager] WatchRewardAndContinue clicked");

        if (waitingReward) return;
        waitingReward = true;
        rewardEarnedThisAd = false;

        // ボタン無効化
        var btn = buttonContinueAd != null ? buttonContinueAd.GetComponent<Button>() : null;
        if (btn != null) btn.interactable = false;

        if (AdManager.Instance == null)
        {
            Debug.LogError("[RingDropManager] AdManager.Instance is null");
            waitingReward = false;
            if (btn != null) btn.interactable = true;
            return;
        }

        if (!AdManager.Instance.IsRewardReady())
        {
            Debug.LogWarning("[RingDropManager] Reward not ready -> Load and unlock");
            waitingReward = false;
            if (btn != null) btn.interactable = true;
            return;
        }

        AdManager.Instance.ShowRewardForExtraThrow();
    // #if UNITY_EDITOR || UNITY_STANDALONE
    // _skipNextSelectClickFrames = 10; // ★広告を開く前にも、選択クリックを捨てる
    // #endif
    //     Debug.Log("[RingDropManager] WatchRewardAndContinue clicked");
    //     // ★広告を開く直前にも入力を止める
    //     _inputBlockedUntil = Time.unscaledTime + inputBlockSecondsAfterAd;

    //     // 連打防止
    //     if (waitingReward) return;
    //     waitingReward = true;
    //     //_rewardGrantedThisAd = false;
    //     _rewardEarnedThisAd = false;   // ★今回の広告ではまだ報酬なし

    //     // ボタンを押せなくする
    //     var btn = buttonContinueAd.GetComponent<Button>();
    //     if (btn != null) btn.interactable = false;

    //     if (AdManager.Instance == null)
    //     {
    //         Debug.LogError("[RingDropManager] AdManager.Instance is null");
    //         waitingReward = false;
    //         if (btn != null) btn.interactable = true;
    //         return;
    //     }

    //     // 広告が未準備ならロードして終了（無反応防止）
    //     if (!AdManager.Instance.IsRewardReady())
    //     {
    //         Debug.LogWarning("[RingDropManager] Reward not ready. Please wait...");
    //         // 次の更新ルーチンで押せるようになる想定
    //         waitingReward = false;
    //         return;
    //     }

    //     // ここで広告を表示（視聴完了時は OnExtraThrowGranted が呼ばれる）
    //     AdManager.Instance.ShowRewardForExtraThrow();
    }

    // ★視聴完了（報酬付与）で呼ばれる
    // private void OnExtraThrowGranted()
    // {
    //     _rewardGrantedThisAd = true;
    //     if (!waitingReward) return;
    //     waitingReward = false;

    //     Debug.Log("[RingDropManager] Reward granted -> +1 throw and resume");

    //     StopContinueAdInteractableRoutine();

    //     // +1回
    //     maxThrows++;
    //     UpdateThrowCountUI();

    //     // UIを閉じる
    //     buttonShowResults.SetActive(false);
    //     buttonContinueAd.SetActive(false);
    //     gameOverPanel.SetActive(false);
    //     if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(false);

    //     // ★投げられる状態へ戻す
    //     isDropping = false;
    //     waitingForNextThrow = false;

    //       // ★ここから追加
    //     _inputBlockedUntil = Time.unscaledTime + inputBlockSecondsAfterAd;

    //     // タッチ位置のキャッシュを消す（古い場所を使わない）
    //     hasLastHitPoint = false;

    //     // 次のTouch Endを一回だけ無視（あなたの既存仕組みを活かす）
    //     suppressNextTouchEnd = true;

    //     // UIの選択状態も外す（稀にボタンが“押されっぱなし”になる対策）
    //     if (EventSystem.current != null)
    //         EventSystem.current.SetSelectedGameObject(null);
    //         // ★ここから追加（最重要）
    //     _blockSelectUntilFingerReleased = true;

    //     // 古いヒット位置を消す（これをやらないと「前の座標」で確定しがち）
    //     hasLastHitPoint = false;
    //     suppressNextTouchEnd = true;
    //     ringMarker.SetActive(false);

    //     // 既存：投げられる状態へ
    //     isDropping = false;
    //     waitingForNextThrow = false;

    //     _skipNextSelectClickFrames = 10; // ★10フレーム分、選択クリックを無効化（PCでも確実）

    //     ReturnToTopView();
    // }
    private void OnExtraThrowGranted()
    {
        //rewardEarnedThisAd = false;

        // if (!waitingReward) return;
        // Debug.Log("[RingDropManager] Reward earned (wait close)");
        // _rewardEarnedThisAd = true;
        if (!waitingReward) return;

        Debug.Log("[RingDropManager] Reward earned -> resume");

        rewardEarnedThisAd = true;
        waitingReward = false;

        // +1
        maxThrows++;
        UpdateThrowCountUI();

        // UI閉じる
=======

    public void ShowReword()
    {
        //リワード呼ぶ
        AdManager.Instance.ShowRewarded();
    }

    void OnEnable()
    {
        AdmobLibrary.OnReward += OnRewarded;   // 報酬受け取り
    }

    void OnDisable()
    {
        AdmobLibrary.OnReward -= OnRewarded;
    }

    //リワードを受け取った
    private void OnRewarded(double amount)
    {
        Debug.Log("OnRewarded リワードを受け取ったので報酬を反映する" );
        // ここで「追加投球」を付与
        maxThrows += 1;
        UpdateThrowCountUI();

        // 次の投球へ戻す
>>>>>>> 213fabbce76f4c5986605969e38941d1711e15c6
        buttonShowResults.SetActive(false);
        buttonContinueAd.SetActive(false);
        gameOverPanel.SetActive(false);
        if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(false);

<<<<<<< HEAD
        // 状態戻す
        isDropping = false;
        waitingForNextThrow = false;
        _ignoreNextSelectOnce = true;

        // ★広告復帰後：一定時間＋1クリック無視で落下選択を防ぐ
        _inputBlockedUntil = Time.unscaledTime + 0.8f;  // 0.6〜1.2で調整
        _ignoreNextSelectOnce = true;
        _blockSelectThisFrame = true;
        suppressNextTouchEnd = true;
        hasLastHitPoint = false;
        if (ringMarker != null) ringMarker.SetActive(false);

        ReturnToTopView();

        rewardEarnedThisAd = false;
        // StartCoroutine(ResumeSelectAfterInputReleased());
    }

    // ★広告を閉じたが、報酬が来なかった場合の解除処理
    // private void OnRewardClosed()
    // {
    //     // ★報酬ありで閉じたなら何もしない（再開処理は OnExtraThrowGranted 側で完了している）
    //     if (_rewardGrantedThisAd)
    //     {
    //         _rewardGrantedThisAd = false; // 次のために戻す
    //         return;
    //     }
    // }
    //     if (!waitingReward) return;

    //     Debug.Log("[RingDropManager] Reward closed without grant -> unlock UI");

    //     waitingReward = false;

    //     // リワードボタンを戻す
    //     if (buttonContinueAd != null)
    //     {
    //         var btn = buttonContinueAd.GetComponent<Button>();
    //         if (btn != null) btn.interactable = true;
    //     }

    //     // 入力ブロック解除（念のため）
    //     _inputBlockedUntil = Time.unscaledTime + 0.2f;
    //     suppressNextTouchEnd = true;

    //     if (EventSystem.current != null)
    //         EventSystem.current.SetSelectedGameObject(null);
    // }

    // private void OnRewardClosed()
    // {
    //     // 待ってないなら無視
    //     if (!waitingReward) return;

    //     // ★共通：閉じた直後の入力を吸収（PC/モバイル両方）
    //     _inputBlockedUntil = Time.unscaledTime + inputBlockSecondsAfterAd;
    //     suppressNextTouchEnd = true;
    //     _blockSelectUntilFingerReleased = true;
    //     hasLastHitPoint = false;
    //     ringMarker.SetActive(false);
    // #if UNITY_EDITOR || UNITY_STANDALONE
    //     _skipNextSelectClickFrames = 10;
    // #endif
    //     if (EventSystem.current != null)
    //         EventSystem.current.SetSelectedGameObject(null);

    //     if (_rewardEarnedThisAd)
    //     {
    //         // ===== 報酬あり：ここで初めて再開 =====
    //         Debug.Log("[RingDropManager] Reward closed with grant -> resume");

    //         waitingReward = false;
    //         _rewardEarnedThisAd = false;

    //         StopContinueAdInteractableRoutine();

    //         maxThrows++;
    //         UpdateThrowCountUI();

    //         buttonShowResults.SetActive(false);
    //         buttonContinueAd.SetActive(false);
    //         gameOverPanel.SetActive(false);
    //         if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(false);

    //         isDropping = false;
    //         waitingForNextThrow = false;

    //         ReturnToTopView();
    //         return;
    //     }

    //     // ===== 報酬なし：UIだけ戻す =====
    //     Debug.Log("[RingDropManager] Reward closed without grant -> unlock UI");

    //     waitingReward = false;

    //     if (buttonContinueAd != null)
    //     {
    //         var btn = buttonContinueAd.GetComponent<Button>();
    //         if (btn != null) btn.interactable = true;
    //     }
    // }

    private void OnRewardClosed()
    {
        // // 待ってないなら無視
        // if (!waitingReward) return;

        // Debug.Log($"[RingDropManager] RewardClosed event received. earned={_rewardEarnedThisAd}");

        // // ★ここではUnity操作しない。Updateでまとめて処理する
        // _rewardClosedThisAd = true;
        if (!waitingReward) return;

        if (rewardEarnedThisAd)
        {
            // 報酬ありは OnExtraThrowGranted で復帰済み
            rewardEarnedThisAd = false; // ★次回のためにリセットだけは必須
            // OnExtraThrowGranted で既に復帰済みなので何もしない
            return;
        }

        Debug.Log("[RingDropManager] Reward closed without grant -> unlock");

        waitingReward = false;

        // ボタン戻す
        if (buttonContinueAd != null)
        {
            var btn = buttonContinueAd.GetComponent<Button>();
            if (btn != null) btn.interactable = true;
        }
        _blockSelectThisFrame = true;
        _inputBlockedUntil = Time.unscaledTime + 0.6f;  // 0.4〜0.8で調整
        _ignoreNextSelectOnce = true;
        // ★追加：入力が離れてから選択再開
        //StartCoroutine(ResumeSelectAfterInputReleased());
        suppressNextTouchEnd = true;
        hasLastHitPoint = false;
        if (ringMarker != null) ringMarker.SetActive(false);

    }

    // private void OnRewardClosed()
    // {
    //     // ★報酬ありで閉じたなら何もしない（再開処理は OnExtraThrowGranted 側で完了している）
    //     if (_rewardGrantedThisAd)
    //     {
    //         _rewardGrantedThisAd = false; // 次のために戻す

    //         // ★念のため：閉じた直後の入力を吸収（報酬ありでも誤タップ防止）
    //         _inputBlockedUntil = Time.unscaledTime + inputBlockSecondsAfterAd;
    //         suppressNextTouchEnd = true;
    //         _blockSelectUntilFingerReleased = true;
    //         hasLastHitPoint = false;
    //         ringMarker.SetActive(false);

    // #if UNITY_EDITOR || UNITY_STANDALONE
    //         _skipNextSelectClickFrames = 10;
    // #endif
    //         if (EventSystem.current != null)
    //             EventSystem.current.SetSelectedGameObject(null);

    //         return;
    //     }

    //     // ★報酬なしで閉じた場合：待機状態じゃなければ無視
    //     if (!waitingReward) return;

    //     Debug.Log("[RingDropManager] Reward closed without grant -> unlock UI");

    //     waitingReward = false;

    //     // リワードボタンを戻す
    //     if (buttonContinueAd != null)
    //     {
    //         var btn = buttonContinueAd.GetComponent<Button>();
    //         if (btn != null) btn.interactable = true;
    //     }

    //     // ★閉じた直後の入力を吸収（報酬なしでも誤タップ防止）
    //     _inputBlockedUntil = Time.unscaledTime + inputBlockSecondsAfterAd;
    //     suppressNextTouchEnd = true;
    //     _blockSelectUntilFingerReleased = true;
    //     hasLastHitPoint = false;
    //     ringMarker.SetActive(false);

    // #if UNITY_EDITOR || UNITY_STANDALONE
    //     _skipNextSelectClickFrames = 10;
    // #endif

    //     if (EventSystem.current != null)
    //         EventSystem.current.SetSelectedGameObject(null);
    // }

    // =========================
    // （任意）広告を見ずに続行する処理は使わない想定
    // もし既存で割り当て済みなら、OnClickは WatchRewardAndContinue に変更してください
    // =========================
=======
        ReturnToTopView();
    }

>>>>>>> 213fabbce76f4c5986605969e38941d1711e15c6
    public void ContinueWithAd()
    {
        Debug.LogWarning("[RingDropManager] ContinueWithAd() is legacy. Use WatchRewardAndContinue().");
        // ここは残しておくが、広告なしで+1するだけになるので使わない方が良い
        maxThrows++;
        UpdateThrowCountUI();
        ReturnToTopView();

        buttonShowResults.SetActive(false);
        buttonContinueAd.SetActive(false);
        gameOverPanel.SetActive(false);
        if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(false);

        isDropping = false;
        waitingForNextThrow = false;
    }

    private void ReturnToTopView()
    {
        topDownCamera.transform.position = topDownInitialPosition;
        topDownCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);

        canSelectPosition = true;
        waitingForNextThrow = false;
        hasLastHitPoint = false;

    }

    public void ReturnToTitle()
    {
        // Debug.Log("[RingDropManager] ReturnToTitle called");
        // SceneManager.LoadScene("Title");
         Debug.Log("[RingDropManager] ReturnToTitle called");

        // ★Titleに戻ったら出したいのでフラグON
        if (AdManager.Instance != null)
        {
            AdManager.Instance.RequestInterstitialOnNextTitle();
        }

        SceneManager.LoadScene("Title");
    }

    // =========================
    // ★ContinueAdボタンの押せる状態を更新するルーチン
    // =========================
    private void StartContinueAdInteractableRoutine()
    {
        StopContinueAdInteractableRoutine();
        continueBtnRoutine = StartCoroutine(UpdateContinueAdButtonRoutine());
    }

    private void StopContinueAdInteractableRoutine()
    {
        if (continueBtnRoutine != null)
        {
            StopCoroutine(continueBtnRoutine);
            continueBtnRoutine = null;
        }
    }

    private IEnumerator UpdateContinueAdButtonRoutine()
    {
        var btn = buttonContinueAd.GetComponent<Button>();
        while (gameOverPanel != null && gameOverPanel.activeSelf)
        {
            if (btn != null && AdManager.Instance != null)
            {
                btn.interactable = AdManager.Instance.IsRewardReady();
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    // private IEnumerator ResumeSelectAfterInputReleased()
    // {
    //     // まずは今フレームの入力を捨てる
    //     yield return null;

    //     // 押しっぱなしが残っている間は待つ（マウス/タッチ両対応）
    //     while (Input.GetMouseButton(0) || Input.touchCount > 0)
    //         yield return null;

    //     // 念のため1フレーム余裕
    //     yield return null;

    //     // ここで初めて選択可能に戻す
    //     canSelectPosition = true;

    //     // 余波対策
    //     suppressNextTouchEnd = true;   // タッチ用（あなたの既存機構）
    //     _ignoreNextSelectOnce = true;  // マウス用（あなたの既存機構）
    // }

}

