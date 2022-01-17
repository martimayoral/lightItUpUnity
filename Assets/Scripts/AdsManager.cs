using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
using System;

public class AdsManager : MonoBehaviour, IUnityAdsListener
{
    // UNITY ANDROID
    string gameId = "4477799";

    Action onRewardedAdSuccess;

    public bool adsOn = true;
    static public AdsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {

        Advertisement.Initialize(gameId);
        Advertisement.AddListener(this);
    }

    public void PlayAdd()
    {
        if (!adsOn)
            return;

        if (Advertisement.IsReady("Interstitial_Android"))
            Advertisement.Show("Interstitial_Android");
    }

    public void PlayRewardedAdd(Action onSuccess)
    {
        if (!adsOn)
            return;

        onRewardedAdSuccess = onSuccess;

        if (Advertisement.IsReady("Rewarded_Android"))
            Advertisement.Show("Rewarded_Android");
        else
            Debug.Log("Rw not ready");
    }

    public void ShowBanner()
    {
        if (!adsOn)
            return;

        if (Advertisement.IsReady("Banner_Android"))
        {
            Advertisement.Banner.SetPosition(BannerPosition.TOP_CENTER);
            Advertisement.Banner.Show("Banner_Android");
        }
        else
        {
            StartCoroutine(RepeatShowBanner());
        }
    }

    IEnumerator RepeatShowBanner()
    {
        yield return new WaitForSecondsRealtime(1);
        ShowBanner();
    }

    public void HideBanner()
    {
        if (!adsOn)
            return;

        Advertisement.Banner.Hide();
    }


    public void OnUnityAdsReady(string placementId)
    {
        //Debug.Log("Ads are ready: " + placementId);
    }

    public void OnUnityAdsDidError(string message)
    {
        Debug.Log("Ads ERROR: " + message);
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        Debug.Log("Add started: " + placementId);
    }

    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (!adsOn)
            return;

        if (placementId == "Rewarded_Android" && showResult == ShowResult.Finished)
        {
            Debug.Log("Rewarded add finished");
            Instance.onRewardedAdSuccess.Invoke();
        }
    }
}
