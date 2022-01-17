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
    void Start()
    {
        switch (slider)
        {
            case eSliders.Music:
                gameObject.GetComponent<Slider>().value = UserConfig.musicVolume * 10f;
                break;
            case eSliders.Sounds:
                gameObject.GetComponent<Slider>().value = UserConfig.soundVolume * 10f;
                break;
            case eSliders.AnimationSpeed:
                gameObject.GetComponent<Slider>().value = (.5f - UserConfig.animationSpeed) * 20f;
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
                //Debug.Log("New Music Volume speed: " + UserConfig.musicVolume);
                break;
            case eSliders.Sounds:
                UserConfig.soundVolume = val / 10f;
                AudioManager.Instance.updateSoundVolume();
                //Debug.Log("New Sound Volume speed: " + UserConfig.soundVolume);
                break;
            case eSliders.AnimationSpeed:
                UserConfig.animationSpeed = .5f - (val / 20f);
                //Debug.Log("New Animation Speed: " + UserConfig.animationSpeed);
                break;
            default:
                Debug.LogError("Slider was not set");
                break;
        }
    }
}
