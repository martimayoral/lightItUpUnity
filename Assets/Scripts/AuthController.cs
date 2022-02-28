using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase;

public class AuthController : MonoBehaviour
{


    FirebaseUser user;
    FirebaseAuth auth;
    public static string username;

    public static AuthController Instance;

    void Awake()
    {
        DBGText.Write("AuthController Awake");
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        Debug.Log("Initializing firebase");
        DBGText.Write("Initializing firebase");

        auth = FirebaseAuth.DefaultInstance;

        auth.StateChanged += Auth_StateChanged;
        Auth_StateChanged(this, null);
    }

    private void Auth_StateChanged(object sender, System.EventArgs e)
    {
        DBGText.Write("Auth State Changed: current user \"" + (auth.CurrentUser != null ? auth.CurrentUser.DisplayName : "null") + "\"");
        Debug.Log("Auth State Changed");

        // first time they enter, no anonymous user
        if (user == null && auth.CurrentUser == null && username != "")
        {
            Debug.Log("Not singed in");
            SignInAnonymously();
            username = "";
        }
        else if (auth.CurrentUser != user)
        {
            bool signedIn = auth.CurrentUser != null;

            // when user signs out
            if (!signedIn && user != null)
            {
                DBGText.Write("Signed out");
                Debug.Log("Signed out");
                updateLogging(false);
                SignInAnonymously(); // sign them as anonymous
            }
            else
            {
                // already signed in
                user = auth.CurrentUser;

                // if it is not anonymous
                if (signedIn && user.DisplayName != "")
                {
                    updateLogging(true);
                    Debug.Log("Signed In as: " + user.DisplayName);
                }
                else
                {
                    updateLogging(false);
                    Debug.Log("Signed in as anonymous");
                }
            }
        }
    }

    void updateLogging(bool signedIn)
    {
        DBGText.Write("Updating logging " + (signedIn ? "(Signed in)" : "(Not signed in)"));
        if (signedIn)
        {
            if (MenuController.Instance)
                MenuController.Instance.SettingsPanelShowInit();
            username = user.DisplayName;
            LanguageManager.ChangeVariable("username", username);
            ToastController.Instance.ToastBlue(LanguageManager.GetTranslation("wellcome") + " " + user.DisplayName);
            DBGText.Write("Wellcome " + user.DisplayName);
        }
        else
        {
            username = "";
        }
        if (MenuController.Instance)
            MenuController.Instance.UpdateLoggingUI();
    }

    public bool isLoggedIn()
    {
        return username != "";
    }


    public void SignInAnonymously()
    {
        if (auth.CurrentUser != null)
        {
            Debug.Log("Trying to sign in anonymously but already logged in as " + auth.CurrentUser.DisplayName);
            return;
        }

        DBGText.Write("Singing in anonymously");

        Debug.Log("Singing in anonymously");

        auth.SignInAnonymouslyAsync().ContinueWith(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            user = auth.CurrentUser;
            username = user.DisplayName;
            Firebase.Auth.FirebaseUser newUser = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);
        });
    }

    public void Login(string email, string pass)
    {
        if (!TextUtils.IsEmail(email))
        {
            ToastController.Instance.ToastRed(LanguageManager.GetTranslation("enter a valid email"));
            return;
        }
        StartCoroutine(ILogin());

        IEnumerator ILogin()
        {
            Debug.Log("LOGGING IN " + email + " " + pass);
            var signInTask = auth.SignInWithEmailAndPasswordAsync(email, pass);

            LoadingScreen.Instance.StartFullScreenSpinner();
            yield return new WaitUntil(() => signInTask.IsCompleted);

            if (signInTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)signInTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "unknown error, please try again";
                switch (error)
                {
                    case AuthError.InvalidEmail:
                        output = "please enter your email";
                        break;
                    case AuthError.MissingPassword:
                        output = "please enter a password";
                        break;
                    case AuthError.WrongPassword:
                    case AuthError.UserNotFound:
                        output = "wrong username or password";
                        break;

                }
                ToastController.Instance.ToastRed(LanguageManager.GetTranslation(output));
            }
            else
            {
                updateLogging(true);
            }
            yield return new WaitForSeconds(0.3f);
            LoadingScreen.Instance.StopAll();
        }
    }

    public void Register(string email, string pass, string rePass, string username)
    {
        if (!TextUtils.IsEmail(email))
        {
            ToastController.Instance.ToastRed(LanguageManager.GetTranslation("enter a valid email"));
            return;
        }
        if (!TextUtils.IsValidUsername(username))
        {
            ToastController.Instance.ToastRed(LanguageManager.GetTranslation("username not valid"));
            return;
        }
        if (!TextUtils.IsValidPassword(pass))
        {
            ToastController.Instance.ToastRed(LanguageManager.GetTranslation("password must have 6 to 20 characters"));
            return;
        }
        if (!pass.Equals(rePass))
        {
            ToastController.Instance.ToastRed(LanguageManager.GetTranslation("passwords doesn't match!"));
            return;
        }

        StartCoroutine(IRegister());

        IEnumerator IRegister()
        {

            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, pass);

            LoadingScreen.Instance.StartFullScreenSpinner();
            yield return new WaitUntil(() => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)registerTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "unknown error, please try again";
                switch (error)
                {
                    case AuthError.InvalidEmail:
                        output = "enter a valid email";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        output = "email already in use";
                        break;
                    case AuthError.WeakPassword:
                        output = "password is not strong enought";
                        break;
                    case AuthError.MissingPassword:
                        output = "please enter a password";
                        break;

                }
                ToastController.Instance.ToastRed(LanguageManager.GetTranslation(output));
            }
            else
            {
                UserProfile profile = new UserProfile
                {
                    DisplayName = username
                };

                var defaultUserTask = user.UpdateUserProfileAsync(profile);

                yield return new WaitUntil(() => defaultUserTask.IsCompleted);

                if (defaultUserTask.Exception != null)
                {
                    user.DeleteAsync();

                    FirebaseException firebaseException = (FirebaseException)registerTask.Exception.GetBaseException();
                    AuthError error = (AuthError)firebaseException.ErrorCode;
                    string output = "unknown error, please try again";
                    switch (error)
                    {
                        case AuthError.SessionExpired:
                            output = "session expired";
                            break;

                    }
                    ToastController.Instance.ToastRed(LanguageManager.GetTranslation(output));
                }
                else
                {
                    DBGText.Write("Register ok; display name: " + user.DisplayName);
                    updateLogging(true);
                    yield return new WaitForSeconds(0.3f);
                    AuthController.username = username;
                    LanguageManager.ChangeVariable("username", username);

                    if (MenuController.Instance)
                        MenuController.Instance.UpdateLoggingUI();
                }
            }
            yield return new WaitForSeconds(0.3f);
            LoadingScreen.Instance.StopAll();

        }
    }

    public void Logout()
    {
        auth.SignOut();
        ToastController.Instance.ToastBlue(LanguageManager.GetTranslation("signed out"));
    }
}
