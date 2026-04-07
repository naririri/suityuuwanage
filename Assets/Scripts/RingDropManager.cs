using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RingDropManager : MonoBehaviour
{
    [Header("Cameras")]
    public Camera topDownCamera;
    public Camera mainCamera;

    [Header("Gameplay")]
    public GameObject ringPrefab;
    public float spawnHeight = 10f;
    public GameObject ringMarker;

    [Header("UI")]
    public GameObject popupUI;
    public TextMeshProUGUI throwCountText;

    public GameObject gameOverPanel;
    public GameObject buttonShowResults;
    public GameObject buttonContinueAd;

    public GameObject resultPanel;
    public GameObject buttonReturnToTitle;
    public TextMeshProUGUI resultScoreText;
    public TextMeshProUGUI resultMessageText;

    public GameObject goldTorus;
    public GameObject crownGold;
    public GameObject crownSilver;

    [Header("Config")]
    public int maxThrows = 5;

    // ---- state ----
    private int currentThrows = 0;
    private Vector3 selectedDropPosition;
    private bool canSelectPosition = true;
    private bool isDropping = false;
    private bool waitingForNextThrow = false;
    private Vector3 topDownInitialPosition;

    // ---- touch helper ----
    private Vector3 lastHitPoint;
    private bool hasLastHitPoint = false;

    // UIボタンを押した指の Ended を1回だけ無視（Confirm/Cancel直後専用）
    private bool suppressNextTouchEnd = false;

    // ---- reward flow ----
    private bool waitingReward = false;
    private bool rewardEarnedThisAd = false;

    // ---- post-ad input block ----
    private float _inputBlockedUntil = 0f;
    private bool _blockSelectUntilFingerReleased = false;
    private bool _blockSelectThisFrame = false;

    // 広告やOSオーバーレイ直後、指が残っている時だけ完全停止
    private bool blockInputUntilFingerUp = false;

    private Coroutine _bindCo;

    void Start()
    {
        topDownInitialPosition = topDownCamera.transform.position;

        popupUI.SetActive(false);
        mainCamera.gameObject.SetActive(false);
        topDownCamera.gameObject.SetActive(true);

        if (ringMarker != null) ringMarker.SetActive(false);

        UpdateThrowCountUI();

        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (buttonShowResults != null) buttonShowResults.SetActive(false);
        if (buttonContinueAd != null) buttonContinueAd.SetActive(false);

        if (resultPanel != null) resultPanel.SetActive(false);
        if (resultScoreText != null) resultScoreText.gameObject.SetActive(false);
        if (resultMessageText != null) resultMessageText.gameObject.SetActive(false);
        if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(false);
    }

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

    void Update()
    {
#if !UNITY_EDITOR
        // 広告などの直後：指が完全に離れるまで入力処理を止める
        if (blockInputUntilFingerUp)
        {
            if (Input.touchCount == 0)
                blockInputUntilFingerUp = false;
            else
                return;
        }
#endif

        // 1フレームだけ吸収
        if (_blockSelectThisFrame)
        {
            _blockSelectThisFrame = false;
            return;
        }

        // 一定時間入力捨て
        if (Time.unscaledTime < _inputBlockedUntil) return;

        // 落下中は入力無効
        if (isDropping) return;

        // 広告直後は「指が完全に離れるまで」選択禁止
        if (_blockSelectUntilFingerReleased)
        {
            if (Input.touchCount == 0 && !Input.GetMouseButton(0))
                _blockSelectUntilFingerReleased = false;
            else
                return;
        }

#if UNITY_EDITOR
        HandleMouseInput();
#else
        if (Application.isMobilePlatform) HandleTouchInput();
        else HandleMouseInput();
#endif
    }

    // ========================
    // Input (Mouse)
    // ========================
    private void HandleMouseInput()
    {
        // 次の投球へ戻る処理を優先
        if (waitingForNextThrow && Input.GetMouseButtonDown(0))
        {
            ReturnToTopView();
            return;
        }

        // marker follow
        if (canSelectPosition && !popupUI.activeSelf && currentThrows < maxThrows)
        {
            Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (ringMarker != null)
                {
                    ringMarker.SetActive(true);
                    ringMarker.transform.position = hit.point + Vector3.up * 0.1f;
                }
            }
        }
        else
        {
            if (ringMarker != null) ringMarker.SetActive(false);
        }

        if (canSelectPosition && !popupUI.activeSelf && Input.GetMouseButtonDown(0) && currentThrows < maxThrows)
        {
            Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                selectedDropPosition = hit.point;
                popupUI.SetActive(true);
            }
        }
    }

    // ========================
    // Input (Touch)
    // ========================
    private void HandleTouchInput()
    {
        // 次の投球へ戻る処理を優先
        if (waitingForNextThrow && Input.touchCount > 0)
        {
            Touch rt = Input.GetTouch(0);
            if (rt.phase == TouchPhase.Began)
            {
                ReturnToTopView();
                return;
            }
        }

        if (!(canSelectPosition && !popupUI.activeSelf && currentThrows < maxThrows))
        {
            if (ringMarker != null) ringMarker.SetActive(false);
            return;
        }

        if (Input.touchCount == 0)
        {
            if (ringMarker != null) ringMarker.SetActive(false);
            return;
        }

        Touch t = Input.GetTouch(0);

        // UI上のタッチは無視
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(t.fingerId))
            return;

        // Confirm/Cancel直後の Ended だけ無視
        if (suppressNextTouchEnd && (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled))
        {
            suppressNextTouchEnd = false;
            return;
        }

        // marker follow
        Ray ray = topDownCamera.ScreenPointToRay(t.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (ringMarker != null)
            {
                ringMarker.SetActive(true);
                ringMarker.transform.position = hit.point + Vector3.up * 0.1f;
            }
            lastHitPoint = hit.point;
            hasLastHitPoint = true;
        }

        // 指を離した位置で確定
        if (t.phase == TouchPhase.Ended && hasLastHitPoint)
        {
            selectedDropPosition = lastHitPoint;
            popupUI.SetActive(true);
            hasLastHitPoint = false;
        }
    }

    // ========================
    // UI
    // ========================
    private void UpdateThrowCountUI()
    {
        int remaining = maxThrows - currentThrows;
        if (throwCountText != null) throwCountText.text = $"残り：{remaining}回";
    }

    public void ConfirmDrop()
    {
        popupUI.SetActive(false);
        canSelectPosition = false;
        isDropping = true;

        // Confirm/Cancel直後だけ Ended を無視
        hasLastHitPoint = false;
        suppressNextTouchEnd = true;

        StartCoroutine(DropSequence());
    }

    public void CancelDrop()
    {
        popupUI.SetActive(false);
        canSelectPosition = true;

        hasLastHitPoint = false;
        suppressNextTouchEnd = true;
    }

    private IEnumerator DropSequence()
    {
        topDownCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);

        Vector3 spawnPos = selectedDropPosition + Vector3.up * spawnHeight;
        Instantiate(ringPrefab, spawnPos, Quaternion.identity);

        currentThrows++;
        UpdateThrowCountUI();

        if (currentThrows >= maxThrows)
        {
            yield return new WaitForSeconds(10f);

            if (gameOverPanel != null) gameOverPanel.SetActive(true);
            yield return new WaitForSeconds(1f);

            if (buttonShowResults != null) buttonShowResults.SetActive(true);
            if (buttonContinueAd != null) buttonContinueAd.SetActive(true);
            yield break;
        }

        isDropping = false;
        waitingForNextThrow = true;
    }

    public void ShowResults()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(true);

        int score = (ScoreManager.Instance != null) ? ScoreManager.Instance.totalScore : 0;

        if (resultScoreText != null)
        {
            resultScoreText.text = (ScoreManager.Instance != null) ? $"Score：{score}" : "スコアデータなし";
            resultScoreText.gameObject.SetActive(true);
        }

        if (crownGold != null) crownGold.SetActive(false);
        if (crownSilver != null) crownSilver.SetActive(false);
        if (goldTorus != null) goldTorus.SetActive(false);

        string message;
        if (score >= 180) { message = "輪投げの達人です！"; if (goldTorus != null) goldTorus.SetActive(true); }
        else if (score >= 120) { message = "ラッキーな１日になる！"; if (crownGold != null) crownGold.SetActive(true); }
        else if (score >= 80) { message = "良い事あるかも！"; if (crownSilver != null) crownSilver.SetActive(true); }
        else if (score >= 40) { message = "Good Job！"; }
        else { message = "前向きに！"; }

        if (resultMessageText != null)
        {
            resultMessageText.text = message;
            resultMessageText.gameObject.SetActive(true);
        }

        if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(true);
    }

    // =========================
    // Reward flow
    // =========================
    public void WatchRewardAndContinue()
    {
        if (waitingReward) return;
        waitingReward = true;
        rewardEarnedThisAd = false;

        var btn = buttonContinueAd != null ? buttonContinueAd.GetComponent<Button>() : null;
        if (btn != null) btn.interactable = false;

        if (AdManager.Instance == null)
        {
            waitingReward = false;
            if (btn != null) btn.interactable = true;
            return;
        }

        if (!AdManager.Instance.IsRewardReady())
        {
            waitingReward = false;
            if (btn != null) btn.interactable = true;
            return;
        }

        // 広告を出す直前に、指が残っているなら完全停止
        blockInputUntilFingerUp = true;

        AdManager.Instance.ShowRewardForExtraThrow();
    }

    private void OnExtraThrowGranted()
    {
        waitingReward = false;
        rewardEarnedThisAd = true;

        maxThrows += 1;
        UpdateThrowCountUI();

        // UIを閉じる
        if (popupUI != null) popupUI.SetActive(false);
        if (buttonShowResults != null) buttonShowResults.SetActive(false);
        if (buttonContinueAd != null) buttonContinueAd.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (resultPanel != null) resultPanel.SetActive(false);
        if (buttonReturnToTitle != null) buttonReturnToTitle.SetActive(false);

        // 状態を戻す
        isDropping = false;
        waitingForNextThrow = false;
        canSelectPosition = true;

        ApplyPostAdInputBlock(0.25f);
        ReturnToTopView();
    }

    private void OnRewardClosed()
    {
        if (!waitingReward) return;

        if (rewardEarnedThisAd)
        {
            rewardEarnedThisAd = false;
            return;
        }

        waitingReward = false;

        // ボタンを戻す
        if (buttonContinueAd != null)
        {
            var btn = buttonContinueAd.GetComponent<Button>();
            if (btn != null) btn.interactable = true;
        }

        ApplyPostAdInputBlock(0.25f);
        ReturnToTopView();
    }

    private void ApplyPostAdInputBlock(float seconds)
    {
        _inputBlockedUntil = Time.unscaledTime + seconds;
        _blockSelectThisFrame = true;

        bool pressedNow;
#if UNITY_EDITOR
        pressedNow = Input.GetMouseButton(0);
#else
        pressedNow = Application.isMobilePlatform ? (Input.touchCount > 0) : Input.GetMouseButton(0);
#endif

        // 押されている時だけ「離すまで待つ」
        _blockSelectUntilFingerReleased = pressedNow;

        // ★広告後は Ended を潰さない
        suppressNextTouchEnd = false;

        hasLastHitPoint = false;

        if (ringMarker != null) ringMarker.SetActive(false);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);

#if !UNITY_EDITOR
        // 指が残っている時だけ完全停止
        blockInputUntilFingerUp = pressedNow && Application.isMobilePlatform;
#endif
    }

    private void ReturnToTopView()
    {
        topDownCamera.transform.position = topDownInitialPosition;
        topDownCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);

        canSelectPosition = true;
        waitingForNextThrow = false;
        hasLastHitPoint = false;

        if (ringMarker != null) ringMarker.SetActive(false);
        if (popupUI != null) popupUI.SetActive(false);
    }

    public void ReturnToTitle()
    {
        if (AdManager.Instance != null)
            AdManager.Instance.RequestInterstitialOnNextTitle();

        SceneManager.LoadScene("Title");
    }
}