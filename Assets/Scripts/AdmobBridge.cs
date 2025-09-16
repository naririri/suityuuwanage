using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class AdmobBridge : MonoBehaviour
{
    // シーンをまたいで常駐させたい場合は true
    [SerializeField] bool _dontDestroyOnLoad = true;

    void Awake()
    {
        if (_dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        // 初回初期化（あなたのライブラリを呼ぶ）
        AdmobLibrary.FirstSetting();

        // 任意：バナー表示（起動時に出したい場合）
        // 第1引数のsizeはライブラリ内で無視され、常にアダプティブが使われます
        AdmobLibrary.RequestBanner(AdSize.Banner, AdPosition.Bottom, collapsible: true);

        // リワードを先読み（必要なら）
        AdmobLibrary.LoadReward();

        // インタースティシャルの読込完了イベント（必要なら）
        AdmobLibrary.OnLoadedInterstitial += () =>
        {
            Debug.Log("Interstitial Ready");
        };
    }

    // UIボタンから呼べる“インスタンス”メソッド（InspectorでOnClickに割当OK）
    public void ShowInterstitial()
    {
        AdmobLibrary.PlayInterstitial();
    }

    public void ShowRewarded()
    {
        AdmobLibrary.ShowReward();
    }

    public void HideBanner()
    {
        AdmobLibrary.DestroyBanner();
    }

    // 任意：再読込
    public void ReloadRewarded()
    {
        AdmobLibrary.LoadReward();
    }
}

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// /* 追加 */
// using GoogleMobileAds.Api;

// public class GoogleAdMobBanner : MonoBehaviour
// {
//     private BannerView bannerView;
    
//     public void Start()
//     {
//         // Initialize the Google Mobile Ads SDK.
//         MobileAds.Initialize(initStatus => { });

//         this.RequestBanner();
//     }

//     private void RequestBanner()
//     {
// #if UNITY_ANDROID
//             string adUnitId = "ca-app-pub-3940256099942544/6300978111";
// #elif UNITY_IPHONE
//         string adUnitId = "ca-app-pub-3940256099942544/2934735716";
// #else
//             string adUnitId = "unexpected_platform";
// #endif
//         // Create a 320x50 banner at the top of the screen.
//         this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Top);

//         // Create an empty ad request.
//         AdRequest request = new AdRequest.Builder().Build();

//         // Load the banner with the request.
//         this.bannerView.LoadAd(request);

//     }
// }
