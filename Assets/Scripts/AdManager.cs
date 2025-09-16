using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GoogleMobileAds.Api;
using System;

public class AdManager : MonoBehaviour
{
    public static AdManager Instance { get; private set; }
    [SerializeField] bool dontDestroyOnLoad = true;

    private bool _initialized = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        InitIfNeeded();

        // シーン切替でバナー制御
        SceneManager.sceneLoaded += OnSceneLoaded;

        // リワードが閉じられたら次を自動先読み（あなたのLibrary側でやるなら不要）
        AdmobLibrary.OnReward += _ => { /* 報酬付与は呼び手側で */ };
    }

    private void InitIfNeeded()
    {
        if (_initialized) return;
        AdmobLibrary.FirstSetting(); // 一回だけ
        _initialized = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode _)
    {
        // Titleではバナー表示、それ以外は消す
        if (scene.name == "Title")
        {
            // アダプティブ／下部／必要なら折りたたみ
            AdmobLibrary.RequestBanner(AdSize.Banner, AdPosition.Bottom, collapsible: true);
        }
        else
        {
            AdmobLibrary.DestroyBanner();
        }

        // ゲーム終了時にリワードを使う運用なら、ゲームプレイ中に先読みしておく
        if (scene.name == "Main" || scene.name == "Game")
        {
            AdmobLibrary.LoadReward(); // 複数回呼んでもLibrary側で上書き直しOK
        }
    }

    // 結果ポップアップのボタンはこれを呼ぶ
    public void ShowRewarded()
    {
        AdmobLibrary.ShowReward();
    }

    public bool IsRewardReady() => AdmobLibrary.IsActiveReward();
}
