using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderControler : MonoBehaviour
{
    public enum eSliders
    {
        Music,
        Sounds,
        AnimationSpeed
    }

    public eSliders slider;

    // Start is called before the first frame update
    void Awake()
    {
        switch (slider)
        {
            case eSliders.Music:
                Debug.Log("seting music");
                gameObject.GetComponent<Slider>().value = UserConfig.musicVolume * 10f;
                break;
            case eSliders.Sounds:
                Debug.Log("seting sounds");
                gameObject.GetComponent<Slider>().value = UserConfig.soundVolume * 10f;
                break;
            case eSliders.AnimationSpeed:
                Debug.Log("seting animation: " + ((1f - UserConfig.animationSpeed) * 10f));
                gameObject.GetComponent<Slider>().value = (1f - UserConfig.animationSpeed) * 10f;
                break;
            default:
                Debug.LogError("Slider was not set");
                break;
        }
    }

    public void OnValueChange(float val)
    {
        switch (slider)
        {
            case eSliders.Music:
                UserConfig.musicVolume = val / 10f;
                AudioManager.Instance.updateMusicVolume();
                break;
            case eSliders.Sounds:
                UserConfig.soundVolume = val / 10f;
                AudioManager.Instance.updateSoundVolume();
                break;
            case eSliders.AnimationSpeed:
                UserConfig.animationSpeed = 1f - (val / 10f);
                Debug.Log("New animation speed: " + UserConfig.animationSpeed);
                break;
            default:
                Debug.LogError("Slider was not set");
                break;
        }
    }
}
