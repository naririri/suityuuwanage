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

    // リワード視聴完了通知（RingDropManager が購読）
    public event Action OnExtraThrowGranted;

    // ShowRewardForExtraThrow を呼んだ時だけ true にして、報酬が来たら通知
    private bool _waitingExtraThrow = false;

    // Interstitial待ち表示用
    private Coroutine _coShowInterstitial = null;

    public event Action OnRewardClosed;

    // --- callbackはスレッドが怪しいのでUpdateで処理する ---
    private bool _pendingRewardEarned = false;
    private bool _pendingRewardClosed = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        InitIfNeeded();

        SceneManager.sceneLoaded += OnSceneLoaded;

        // 重複購読防止（Awakeが何度か呼ばれても安全）
        AdmobLibrary.OnReward -= OnRewardEarned;
        AdmobLibrary.OnReward += OnRewardEarned;

        AdmobLibrary.OnRewardClosed -= HandleRewardClosed;
        AdmobLibrary.OnRewardClosed += HandleRewardClosed;
    }

    private void InitIfNeeded()
    {
        if (_initialized) return;
        AdmobLibrary.FirstSetting(); // 初回だけ
        _initialized = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[AdManager] SceneLoaded: {scene.name}");

        if (scene.name == "Title")
        {
            // Title：バナー表示
            AdmobLibrary.RequestBanner(AdSize.Banner, AdPosition.Bottom, collapsible: true);

            // Mainから戻ったときだけインタースティシャル（準備待ちで表示）
            if (_showInterstitialOnNextTitle)
            {
                _showInterstitialOnNextTitle = false;
                TryShowInterstitialOnTitle();
            }
            return;
        }

        // Title以外：バナー消す
        AdmobLibrary.DestroyBanner();

        // Main：リワード先読み（必要ならここで）
        if (scene.name == "main")
        {
            AdmobLibrary.LoadReward();
            // Interstitialもここで先読みしたいなら（InitInterstitialが自動なら不要）
            // ※もし「not ready」が多いなら、ここでロード済み状態を作るのが効果的
            // AdmobLibrary.LoadInterstitial();  ←あなたが実装している場合のみ
        }
        Debug.Log("Test1");
        if (scene.name == "main")
        {
            Debug.Log("Test2");
            AdmobLibrary.LoadReward(); // ← mainで先読み
        }
    }

    // RingDropManagerから呼ぶ：+1投げのためにリワードを出す
    public void ShowRewardForExtraThrow()
    {
        Debug.Log("[AdManager] ShowRewardForExtraThrow");

        if (!AdmobLibrary.IsActiveReward())
        {
            Debug.LogWarning("[AdManager] Reward not ready -> LoadReward");
            AdmobLibrary.LoadReward();
            return;
        }

        _waitingExtraThrow = true;
        AdmobLibrary.ShowReward();
    }

    private void OnRewardEarned(double amount)
    {
        Debug.Log($"[AdManager] OnRewardEarned amount={amount}, waiting={_waitingExtraThrow}");

        if (!_waitingExtraThrow) return;

        _waitingExtraThrow = false;

        // ★ここで直接Invokeしない。Updateで投げる
        _pendingRewardEarned = true;
    }


    public bool IsRewardReady() => AdmobLibrary.IsActiveReward();

    // Titleへ戻ったら出したいのでフラグON（RingDropManagerが呼ぶ）
    public void RequestInterstitialOnNextTitle()
    {
        _showInterstitialOnNextTitle = true;
    }

    // --- Interstitial：準備できるまで待って表示 ---
    private void TryShowInterstitialOnTitle()
    {
        if (_coShowInterstitial != null) StopCoroutine(_coShowInterstitial);
        _coShowInterstitial = StartCoroutine(CoShowInterstitialWhenReady());
    }

    private IEnumerator CoShowInterstitialWhenReady()
    {
        float end = Time.unscaledTime + 3f; // 最大3秒待つ

        while (Time.unscaledTime < end)
        {
            if (AdmobLibrary.IsInterstitialReady())
            {
                AdmobLibrary.PlayInterstitial();
                yield break;
            }
            yield return new WaitForSecondsRealtime(0.25f);
        }

        Debug.LogWarning("[AdManager] Interstitial not ready (timeout)");
    }

    private void HandleRewardClosed()
    {
        Debug.Log("[AdManager] Reward closed (callback)");

        // ★ここで直接Invokeしない。Updateで投げる
        _pendingRewardClosed = true;
    }


    void Update()
    {
        if (_pendingRewardEarned)
        {
            _pendingRewardEarned = false;
            Debug.Log("[AdManager] Dispatch RewardEarned (Update)");
            OnExtraThrowGranted?.Invoke();
        }

        if (_pendingRewardClosed)
        {
            _pendingRewardClosed = false;
            Debug.Log("[AdManager] Dispatch RewardClosed (Update)");
            OnRewardClosed?.Invoke();
        }
    }

}