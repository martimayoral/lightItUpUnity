using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToastController : MonoBehaviour
{
    public TextMeshProUGUI top;
    public TextMeshProUGUI bottom;
    public static ToastController Instance;

    public enum eToastType
    {
        TOP,
        BOTTOM
    }

    Animator anim;
    private void Awake()
    {
        Instance = this;
        anim = GetComponent<Animator>();
    }

    public void ToastRed(string message, eToastType type = eToastType.BOTTOM)
    {
        ToastMessage(message, new Color(1f, .2f, .2f), type);
    }

    public void ToastGreen(string message, eToastType type = eToastType.BOTTOM)
    {
        ToastMessage(message, new Color(.2f, 1, .2f), type);
    }
    public void ToastBlue(string message, eToastType type = eToastType.BOTTOM)
    {
        ToastMessage(message, new Color(.2f, .2f, 1f), type);
    }

    public void ToastWhite(string message, eToastType type = eToastType.BOTTOM)
    {
        ToastMessage(message, Color.white, type);
    }

    public void ToastMessage(string message, Color color, eToastType type)
    {
        switch (type)
        {
            case eToastType.TOP:
                anim.SetTrigger("ToastTop");
                top.SetText(message);
                top.color = color;
                break;
            case eToastType.BOTTOM:
                anim.SetTrigger("ToastBottom");
                bottom.SetText(message);
                bottom.color = color;
                break;
            default:
                break;
        }
    }

}
