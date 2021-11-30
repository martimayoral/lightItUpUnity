using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum medalType
{
    none,
    bronze,
    silver, 
    gold
}

public class GlobalVars : MonoBehaviour
{
    public static Color gold = new Color32(218, 165, 32, 255);
    public static Color silver = new Color32(169, 169, 169, 255);
    public static Color bronze = new Color32(160, 82, 45, 255);
    public static Color none = new Color32(128, 128, 128, 255);
}
