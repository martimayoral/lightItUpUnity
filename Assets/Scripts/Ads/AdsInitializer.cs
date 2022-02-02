using UnityEngine;
using UnityEngine.Advertisements;

public class AdsInitializer : MonoBehaviour, IUnityAdsInitializationListener
{
    //[SerializeField] string _androidGameId;
    //[SerializeField] string _iOSGameId;

    [SerializeField] bool _testMode = false;
    private string _gameId = "4553041";

    public static bool adsInitialized = false;

    void Awake()
    {
        InitializeAds();
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeAds()
    {
        /*
        _gameId = (Application.platform == RuntimePlatform.IPhonePlayer)
            ? _iOSGameId
            : _androidGameId;
        */
        Advertisement.Initialize(_gameId, _testMode, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        DBGText.Write("Unity Ads initialization complete.");
        adsInitialized = true;

    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
        DBGText.Write($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }
}