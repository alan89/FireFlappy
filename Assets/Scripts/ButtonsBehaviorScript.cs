using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonsBehaviorScript : MonoBehaviour
{
    // Textfields 
    public InputField emailField;
    public InputField passwordField;

    // Firebase Auth Def
    protected Firebase.Auth.FirebaseAuth auth;
    protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth =
    new Dictionary<string, Firebase.Auth.FirebaseUser>();

    // Strings
    protected string email = "";
    protected string password = "";
    protected string logText = "";

    // Flag set when a token is being fetched.  This is used to avoid printing the token
    // in IdTokenChanged() when the user presses the get token button.
    private bool fetchingToken = false;

    const int kMaxLogSize = 16382;
    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;

    // When the app starts, check to make sure that we have
    // the required dependencies to use Firebase, and if not,
    // add them if possible.
    public virtual void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                  "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    // Handle initialization of the necessary firebase modules:
    protected void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        auth.IdTokenChanged += IdTokenChanged;
        AuthStateChanged(this, null);
    }

    // Exit if escape (or back, on mobile) is pressed.
    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth.IdTokenChanged -= IdTokenChanged;
        auth = null;
    }


    // Buttons behavior
    public void BtnOpenCreateUserUI()
    {
        SceneManager.LoadScene("createAccount", LoadSceneMode.Single);
    }

    public void BtnCreateAccount()
    {
        DebugLog("email: " + emailField.text + " and your password: " + passwordField.text);
        email = emailField.text;
        password = passwordField.text;
        CreateUserWithEmailAsync();
    }

    public void BtnLogin()
    {
        Debug.Log("Login Clicked");
    }

    public void BtnMainGame()
    {
        Debug.Log("Login Clicked");
        SceneManager.LoadScene("mainGame", LoadSceneMode.Single);
    }



    // Auth Actions
    // Track state changes of the auth object.
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        Firebase.Auth.FirebaseUser user = null;
        if (senderAuth != null) userByAuth.TryGetValue(senderAuth.App.Name, out user);
        if (senderAuth == auth && senderAuth.CurrentUser != user)
        {
            bool signedIn = user != senderAuth.CurrentUser && senderAuth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                DebugLog("Signed out " + user.UserId);
            }
            user = senderAuth.CurrentUser;
            userByAuth[senderAuth.App.Name] = user;
            if (signedIn)
            {
                DebugLog("Signed in " + user.UserId);
                SceneManager.LoadScene("mainGame", LoadSceneMode.Single);
            }
        }
    }

    // Track ID token changes.
    void IdTokenChanged(object sender, System.EventArgs eventArgs)
    {
        Firebase.Auth.FirebaseAuth senderAuth = sender as Firebase.Auth.FirebaseAuth;
        if (senderAuth == auth && senderAuth.CurrentUser != null && !fetchingToken)
        {
            senderAuth.CurrentUser.TokenAsync(false).ContinueWith(
              task => DebugLog(String.Format("Token[0:8] = {0}", task.Result.Substring(0, 8))));
        }
    }

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    protected bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            DebugLog(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            DebugLog(operation + " encounted an error.");
            foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
            {
                string authErrorCode = "";
                Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    authErrorCode = String.Format("AuthError.{0}: ",
                      ((Firebase.Auth.AuthError)firebaseEx.ErrorCode).ToString());
                }
                DebugLog(authErrorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            DebugLog(operation + " completed");
            complete = true;
        }
        return complete;
    }

    // Create a user with the email and password.
    public Task CreateUserWithEmailAsync()
    {
        DebugLog(String.Format("Attempting to create user {0}...", email));

        // This passes the current displayName through to HandleCreateUserAsync
        // so that it can be passed to UpdateUserProfile().  displayName will be
        // reset by AuthStateChanged() when the new user is created and signed in.
        return auth.CreateUserWithEmailAndPasswordAsync(email, password)
          .ContinueWith((task) => {
              if (LogTaskCompletion(task, "User Creation"))
              {
                  var user = task.Result;
                  //DisplayDetailedUserInfo(user, 1);
                  //return UpdateUserProfileAsync(newDisplayName: newDisplayName);
              }
              return task;
          }).Unwrap();
    }

    // Output text to the debug log text field, as well as the console.
    public void DebugLog(string s)
    {
        Debug.Log(s);
        logText += s + "\n";

        while (logText.Length > kMaxLogSize)
        {
            int index = logText.IndexOf("\n", StringComparison.Ordinal);
            logText = logText.Substring(index + 1);
        }
    }

}
