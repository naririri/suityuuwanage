using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }
    [SerializeField] private bool dontDestroyOnLoad = true;

    private bool _initialized = false;

    // Main → Title に戻った時だけ、次のTitleでインタースティシャルを出す
    private static bool _showInterstitialOnNextTitle = false;

    public event Action OnExtraThrowGranted;
    private bool _waitingExtraThrow = false;

    public event Action OnRewardClosed;

    private bool _pendingRewardEarned = false;
    private bool _pendingRewardClosed = false;

    // ★同じTitle滞在中に2回出さないガード
    private bool _interstitialShownThisTitle = false;

    // ★待機コルーチン（重複起動防止）
    private Coroutine _coInterstitial = null;

    // ========= 追加：foregroundガード（ここが本丸） =========
    private bool _isForeground = true;
    private bool _pendingShowReward = false;
    private bool _pendingShowInterstitial = false;
    // =======================================================

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        InitIfNeeded();

        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;

        AdmobLibrary.OnReward -= OnRewardEarned;
        AdmobLibrary.OnReward += OnRewardEarned;

        AdmobLibrary.OnRewardClosed -= HandleRewardClosed;
        AdmobLibrary.OnRewardClosed += HandleRewardClosed;

        // ★重要：インターの「ロード完了イベント」は使わない（2回表示の温床）
        AdmobLibrary.OnLoadedInterstitial -= HandleInterstitialLoaded; // 念のため外す
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        SceneManager.sceneLoaded -= OnSceneLoaded;

        AdmobLibrary.OnReward -= OnRewardEarned;
        AdmobLibrary.OnRewardClosed -= HandleRewardClosed;

        AdmobLibrary.OnLoadedInterstitial -= HandleInterstitialLoaded; // 念のため
    }

    // ========= 追加：foreground判定 =========
    void OnApplicationPause(bool pause)
    {
        _isForeground = !pause;
    }

    void OnApplicationFocus(bool focus)
    {
        _isForeground = focus;
    }
    // =====================================

    private void InitIfNeeded()
    {
        if (_initialized) return;
        AdmobLibrary.FirstSetting(); // 初回だけ
        _initialized = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log($"[AdManager] SceneLoaded: {scene.name}");

        if (scene.name == "Title")
        {
            AdmobLibrary.RequestBanner(AdSize.Banner, AdPosition.Bottom, collapsible: true);

            // ★Titleに入ったら「このTitleでまだ出してない」状態
            // ※Titleに居続ける間は二重表示させない
            if (!_interstitialShownThisTitle && _showInterstitialOnNextTitle)
            {
                _showInterstitialOnNextTitle = false;
                _interstitialShownThisTitle = true;

                // 既に走ってたら止める（念のため）
                if (_coInterstitial != null) StopCoroutine(_coInterstitial);
                _coInterstitial = StartCoroutine(CoShowInterstitialOnceWhenReady_FGGuard());
            }
            return;
        }

        // Title以外に出たらガード解除（次回Titleで出せる）
        _interstitialShownThisTitle = false;

        AdmobLibrary.DestroyBanner();

        if (scene.name == "main")
        {
            AdmobLibrary.LoadReward();
        }
    }

    // RingDropManagerから呼ぶ：+1投げのためにリワードを出す
    public void ShowRewardForExtraThrow()
    {
        //Debug.Log("[AdManager] ShowRewardForExtraThrow");

        if (!AdmobLibrary.IsActiveReward())
        {
            Debug.LogWarning("[AdManager] Reward not ready -> LoadReward");
            AdmobLibrary.LoadReward();
            return;
        }

        _waitingExtraThrow = true;

        // ★ここが重要：フォアグラウンドじゃないなら予約して、復帰後に出す
        if (!_isForeground)
        {
            //Debug.LogWarning("[AdManager] App not in foreground -> queue reward show");
            _pendingShowReward = true;
            return;
        }

        StartCoroutine(CoShowRewardNextFrame());
    }

    private IEnumerator CoShowRewardNextFrame()
    {
        yield return null; // 1フレーム待って安定化

        if (!_isForeground)
        {
            //Debug.LogWarning("[AdManager] Lost foreground before showing reward -> queue");
            _pendingShowReward = true;
            yield break;
        }

        AdmobLibrary.ShowReward();
    }

    private void OnRewardEarned(double amount)
    {
        //Debug.Log($"[AdManager] OnRewardEarned amount={amount}, waiting={_waitingExtraThrow}");
        if (!_waitingExtraThrow) return;

        _waitingExtraThrow = false;
        _pendingRewardEarned = true; // Updateで通知
    }

    public bool IsRewardReady() => AdmobLibrary.IsActiveReward();

    // Titleへ戻ったら出したいのでフラグON（RingDropManagerが呼ぶ）
    public void RequestInterstitialOnNextTitle()
    {
        _showInterstitialOnNextTitle = true;
        //Debug.Log("[AdManager] RequestInterstitialOnNextTitle");
    }

    private void HandleRewardClosed()
    {
        //Debug.Log("[AdManager] Reward closed (callback)");
        _pendingRewardClosed = true; // Updateで通知
    }

    // ★1回だけ表示：readyになるまで待つ（最大20秒）
    // ★さらに：foregroundじゃない間は "待つ"（Showしない）
    private IEnumerator CoShowInterstitialOnceWhenReady_FGGuard()
    {
        // シーン切替直後の不安定さ回避
        yield return new WaitForSecondsRealtime(0.5f);

        float end = Time.unscaledTime + 20f;

        while (Time.unscaledTime < end)
        {
            // ★フォアグラウンドじゃない間はShowしない（Code3対策）
            if (!_isForeground)
            {
                _pendingShowInterstitial = true; // 復帰後に続きをやりたい意思
                yield return null;
                continue;
            }

            if (AdmobLibrary.IsInterstitialReady())
            {
                //Debug.Log("[AdManager] Interstitial ready -> PlayInterstitial");

                // 1フレーム遅らせて安定化（OSの切替/入力余波を避ける）
                yield return null;

                if (!_isForeground)
                {
                    _pendingShowInterstitial = true;
                    yield break;
                }

                AdmobLibrary.PlayInterstitial(); // ここは「1回だけ」
                yield break;
            }

            yield return new WaitForSecondsRealtime(0.25f);
        }

       // Debug.LogWarning("[AdManager] Interstitial not ready (timeout)");
    }

    // ★もう使わない（購読もしない）が、コンパイル用に残しておく
    private void HandleInterstitialLoaded() { }

    void Update()
    {
        // ========= 追加：foreground復帰後に予約分を出す =========
        if (_isForeground)
        {
            if (_pendingShowReward)
            {
                // Rewardは「準備済み前提」で予約が立つが、念のためチェック
                _pendingShowReward = false;
                if (AdmobLibrary.IsActiveReward())
                {
                    //Debug.Log("[AdManager] Foreground restored -> show queued reward");
                    StartCoroutine(CoShowRewardNextFrame());
                }
                else
                {
                    //Debug.LogWarning("[AdManager] Queued reward but not ready -> LoadReward");
                    AdmobLibrary.LoadReward();
                }
            }

            if (_pendingShowInterstitial)
            {
                // interstitialはTitleでの待機コルーチン側で処理される想定だが、
                // “復帰したのに止まってしまった”ケースだけ軽く後押しする
                _pendingShowInterstitial = false;

                // Title滞在中で、まだこのTitleで出してないなら、待機を再開
                var active = SceneManager.GetActiveScene().name;
                if (active == "Title" && !_interstitialShownThisTitle)
                {
                    // ここに入るのは基本レア。安全のためにガードも維持。
                    _interstitialShownThisTitle = true;
                    if (_coInterstitial != null) StopCoroutine(_coInterstitial);
                    _coInterstitial = StartCoroutine(CoShowInterstitialOnceWhenReady_FGGuard());
                }
            }
        }
        // =======================================================

        if (_pendingRewardEarned)
        {
            _pendingRewardEarned = false;
            //Debug.Log("[AdManager] Dispatch RewardEarned (Update)");
            OnExtraThrowGranted?.Invoke();
        }

        if (_pendingRewardClosed)
        {
            _pendingRewardClosed = false;
            //Debug.Log("[AdManager] Dispatch RewardClosed (Update)");
            OnRewardClosed?.Invoke();
        }
    }
}
