using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using System;
using System.Collections;

public class RewardedAds : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    //[SerializeField] string _iOSAdUnitId = "Rewarded_iOS";
    string _adUnitId = null; // This will remain null for unsupported platforms

    bool adLoaded;

    Action onSuccessAction;

    public static RewardedAds Instance;

    void Awake()
    {
        // Get the Ad Unit ID for the current platform:
#if UNITY_IOS
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
#endif

        DontDestroyOnLoad(this);

        adLoaded = false;
        Instance = this;

        StartCoroutine(CheckIfLoaded());

        IEnumerator CheckIfLoaded()
        {
            yield return new WaitForSeconds(3);
            Debug.Log($"RA: checking if loaded ({AdsInitializer.adsInitialized})");
            DBGText.Write($"RA: checking if loaded ({AdsInitializer.adsInitialized})");

            if (AdsInitializer.adsInitialized)
                adLoaded = true;
            else
            {
                StartCoroutine(CheckIfLoaded());
            }
        }
    }

    // Load content to the Ad Unit:
    public void LoadAd()
    {
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }

    // If the ad successfully loads, add a listener to the button and enable it:
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log("Ad Loaded: " + adUnitId);

        if (adUnitId.Equals(_adUnitId))
            adLoaded = true;

    }

    // Implement a method to execute when the user clicks the button:
    public void ShowAd(Action onSuccess)
    {
        Debug.Log("Show rewarded ad? " + adLoaded.ToString());
        if (adLoaded)
        {
            onSuccessAction = onSuccess;
            // Then show the ad:
            Advertisement.Show(_adUnitId, this);
        }
    }

    // Implement the Show Listener's OnUnityAdsShowComplete callback method to determine if the user gets a reward:
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED) && onSuccessAction != null)
        {
            Debug.Log($"Unity Ads Rewarded Ad Completed");
            // Grant a reward.
            onSuccessAction.Invoke();

            onSuccessAction = null;

            //currentRewardType = eRewardType.NONE;

            // Load another ad:
            Advertisement.Load(_adUnitId, this);
            adLoaded = false;
        }
    }


    // Implement Load and Show Listener error callbacks:
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Use the error details to determine whether to try to load another ad.
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        // Use the error details to determine whether to try to load another ad.
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }

    void OnDestroy()
    {
        // Clean up the button listeners:
        Debug.Log("Removing listener");
        // _showAdButton.onClick.RemoveAllListeners();
    }
}