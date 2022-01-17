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
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        username = "";
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        Debug.Log("Initializing firebase");

        auth = FirebaseAuth.DefaultInstance;

        auth.StateChanged += Auth_StateChanged;
        Auth_StateChanged(this, null);
    }

    private void Auth_StateChanged(object sender, System.EventArgs e)
    {
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
                    Debug.Log("Signed in as anonymous");
            }
        }
    }

    void updateLogging(bool signedIn)
    {
        if (signedIn)
        {
            if (MenuController.Instance)
                MenuController.Instance.SettingsPanelShowInit();
            username = user.DisplayName;
            ToastController.Instance.ToastBlue("Wellcome " + user.DisplayName);
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
            ToastController.Instance.ToastRed("Enter a valid email");
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
                string output = "Unknown Error, Please Try Again";
                switch (error)
                {
                    case AuthError.InvalidEmail:
                        output = "Please enter your Email";
                        break;
                    case AuthError.MissingPassword:
                        output = "Please enter a password";
                        break;
                    case AuthError.WrongPassword:
                    case AuthError.UserNotFound:
                        output = "Wrong username or password";
                        break;

                }
                ToastController.Instance.ToastRed(output);
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
            ToastController.Instance.ToastRed("Enter a valid email");
            return;
        }
        if (!TextUtils.IsValidUsername(username))
        {
            ToastController.Instance.ToastRed("Username not valid");
            return;
        }
        if (!TextUtils.IsValidPassword(pass))
        {
            ToastController.Instance.ToastRed("Password must have 6 to 20 characters");
            return;
        }
        if (!pass.Equals(rePass))
        {
            ToastController.Instance.ToastRed("Passwords doesn't match!");
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
                string output = "Unknown Error, Please Try Again";
                switch (error)
                {
                    case AuthError.InvalidEmail:
                        output = "Enter a valid email";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        output = "Email already in use";
                        break;
                    case AuthError.WeakPassword:
                        output = "Password is not strong enought";
                        break;
                    case AuthError.MissingPassword:
                        output = "Please enter a password";
                        break;

                }
                ToastController.Instance.ToastRed(output);
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
                    string output = "Unknown Error, Please Try Again";
                    switch (error)
                    {
                        case AuthError.SessionExpired:
                            output = "Session expired";
                            break;

                    }
                    ToastController.Instance.ToastRed(output);
                }
                else
                {
                    updateLogging(true);
                }
            }
            yield return new WaitForSeconds(0.3f);
            LoadingScreen.Instance.StopAll();

        }
    }

    public void Logout()
    {
        auth.SignOut();
        ToastController.Instance.ToastBlue("Signed out");
    }
}
