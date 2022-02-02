using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DBGText : MonoBehaviour
{
    public static TextMeshProUGUI text;
    public TextMeshProUGUI thisText;

    static string messages;
    readonly static bool DBG = false;

    private void Awake()
    {
        text = thisText;
        text.text = messages ?? "";

        if (DBG)
            DontDestroyOnLoad(this);
        else
            Destroy(this);
    }

    public static void Write(string msg)
    {
        if (DBG)
            messages += "\n" + msg;
    }

    private void Update()
    {
        if (DBG)
            text.text = messages;
    }
}
