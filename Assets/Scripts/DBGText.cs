using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DBGText : MonoBehaviour
{
    public static TextMeshProUGUI text;
    static string messages;
    readonly static bool DBG = false;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.text = messages ?? "";
    }

    public static void Write(string msg)
    {
        if (DBG)
            messages += "\n" + msg;
    }

    private void Update()
    {
        text.text = messages;
    }
}
