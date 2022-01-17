using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ToastController : MonoBehaviour
{
    public TextMeshProUGUI text;
    public static ToastController Instance;

    Animator anim;
    private void Awake()
    {
        Instance = this;
        anim = GetComponent<Animator>();
    }

    public void ToastRed(string message)
    {
        ToastMessage(message, new Color(1f, .2f, .2f));
    }

    public void ToastGreen(string message)
    {
        ToastMessage(message, new Color(.2f, 1, .2f));
    }
    public void ToastBlue(string message)
    {
        ToastMessage(message, new Color(.2f, .2f, 1f));
    }

    public void ToastWhite(string message)
    {
        ToastMessage(message, Color.white);
    }

    public void ToastMessage(string message, Color color)
    {
        anim.SetTrigger("Toast");
        text.SetText(message);
        text.color = color;
    }

}
