using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AuthResources : MonoBehaviour
{

    [SerializeField] TMP_InputField loginEmail;
    [SerializeField] TMP_InputField loginPass;

    [SerializeField] TMP_InputField registerUsername;
    [SerializeField] TMP_InputField registerEmail;
    [SerializeField] TMP_InputField registerPass;
    [SerializeField] TMP_InputField registerRepeatPass;



    public void LogIn()
    {
        AuthController.Instance.Login(loginEmail.text, loginPass.text);
    }
    public void Register()
    {
        AuthController.Instance.Register(registerEmail.text,registerPass.text, registerRepeatPass.text, registerUsername.text);
    }

    public void LogOut()
    {
        AuthController.Instance.Logout();
    }

}
