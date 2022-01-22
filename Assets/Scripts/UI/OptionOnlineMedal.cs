using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionOnlineMedal : MonoBehaviour
{
    public medalType medalType;
    Image image;

    // Start is called before the first frame update
    void Start()
    {
        image = GetComponent<Image>();

        SetColor(UserConfig.onlineMedalsOptions[(int)medalType]);
    }

    public void Toggle()
    {
        SetEnabled(!UserConfig.onlineMedalsOptions[(int)medalType]);
    }

    public void SetEnabled(bool enable)
    {
        if (enable == UserConfig.onlineMedalsOptions[(int)medalType])
            return;

        UserConfig.onlineMedalsOptions[(int)medalType] = enable;
        SetColor(enable);

        MenuController.Instance.RefreshUserLevels();
        SaveUserConfig.SaveUserConfigData();
    }

    void SetColor(bool enable)
    {
        if (enable)
            image.color = new Color(1f, 1f, 1f, 1f);
        else
            image.color = new Color(1f, 1f, 1f, .25f);
    }
}
