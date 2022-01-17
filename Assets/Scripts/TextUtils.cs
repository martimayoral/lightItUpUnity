using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public static class TextUtils
{
    public static bool IsEmail(string str)
    {
        return Regex.IsMatch(str, "([a-zA-Z0-9]{1,20})[@]([a-zA-Z0-9]{2,10})[.]([a-zA-Z0-9]{2,10})");
    }
    public static bool IsValidPassword(string str)
    {
        return Regex.IsMatch(str, "(.{6,20})");
    }
    public static bool IsValidUsername(string str)
    {
        return Regex.IsMatch(str, "([a-zA-Z0-9]{3,15})");
    }
}
