using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum medalType
{
    //JUST_TRIED = -1,
    NONE = 0,
    BRONZE = 1,
    SILVER = 2, 
    GOLD = 3
}

public enum directions
{
    UP,
    BOTTOM,
    LEFT,
    RIGHT
}

public static class GlobalVars
{
    public readonly static Color gold = new Color32(218, 165, 32, 255);
    public readonly static Color silver = new Color32(169, 169, 169, 255);
    public readonly static Color bronze = new Color32(88, 46, 8, 255);
    public readonly static Color none = new Color32(79, 79, 79, 255);
}
