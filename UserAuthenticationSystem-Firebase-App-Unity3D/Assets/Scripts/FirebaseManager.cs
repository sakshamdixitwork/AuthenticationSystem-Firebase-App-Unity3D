using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;
using System;
using UnityEngine.UI;
using Facebook.Unity;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser user;
    [Space(5f)]

    [Header("Login Reference")]
    [SerializeField]
    private TMP_InputField loginEmail;
    [SerializeField]
    private TMP_InputField loginPassword;
    [SerializeField]
    private TMP_Text loginInvalidOutputText;
    [SerializeField]
    private TMP_Text loginValidOutputText;
    [Space(5f)]
    

    [Header("Register Reference")]
    [SerializeField]
    private TMP_InputField registerUsername;
    [SerializeField]
    private TMP_InputField registerEmail;
    [SerializeField]
    private TMP_InputField registerPassword;
    [SerializeField]
    private TMP_InputField registerConfirmPassword;
    [SerializeField]
    private TMP_Text registerInvalidOutputText;
    [SerializeField]
    private TMP_Text registerValidOutputText;
    [Space(5f)]

    [Header("ForgotPassword Reference")]
    [SerializeField]
    private TMP_InputField forgotPassEmail;
    [SerializeField]
    private TMP_Text forgotPassEmailText;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if(instance == null)
        {
            instance = this;
        } else if(instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }

    }

    private void Start()
    {
        StartCoroutine(CheckAndFixDependencies());

    }

    private IEnumerator CheckAndFixDependencies()
    {
        var checkAndFixDependenciesTask = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(predicate: () => checkAndFixDependenciesTask.IsCompleted);

        var dependencyResult = checkAndFixDependenciesTask.Result;

        if(dependencyResult == DependencyStatus.Available)
        {
            InitializeFirebase();
        }
        else
        {
            Debug.Log($"Cound not resolve all firebase dependencies: {dependencyResult}");
        }
    }


    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if(auth.CurrentUser != null)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            
            if(!signedIn && user != null)
            {
                Debug.Log("Signed Out");
            }

            user = auth.CurrentUser;

            if(signedIn)
            {
                Debug.Log($"Signed In: {user.DisplayName}");
            }
        
        }
    }

    public void ClearOutput()
    {
        loginInvalidOutputText.text = "";
        loginValidOutputText.text = "";
        registerInvalidOutputText.text = "";
        registerValidOutputText.text = "";
    }

    public void LoginButton()
    {
        StartCoroutine(LoginLogic(loginEmail.text, loginPassword.text));
    }

    public void RegisterButton()
    {
        StartCoroutine(RegisterLogic(registerUsername.text, registerEmail.text, registerPassword.text, registerConfirmPassword.text));
    }

    public void ResetPassword()
    {
        StartCoroutine(ForgotPassword(forgotPassEmail.text));
    }

    public void EmailVerified()
    {
        if(user != null)
        {
            if(user.IsEmailVerified)
            {
                AuthUIManager.instance.LoginUI();
            } else
            {
                StartCoroutine(SendVerificationEmail());
            }
        }
        else
        {
            AuthUIManager.instance.LoginUI();
        }
    }

    private IEnumerator LoginLogic(string _email, string _password)
    {
        ClearOutput();

        if (_email == "")
        {
            loginInvalidOutputText.text = "Please Enter Email!";
        }
        else if (_password == "")
        {
            loginInvalidOutputText.text = "Please Enter Password!";
        }
        else
        {
            Credential credential = EmailAuthProvider.GetCredential(_email, _password);

            var loginTask = auth.SignInWithCredentialAsync(credential);

            yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

            if (loginTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)loginTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "Unknown Error, Please Try Again";

                switch (error)
                {
                    case AuthError.UserNotFound:
                        output = "Account Does Not Exist";
                        break;

                    case AuthError.WrongPassword:
                        output = "Incorrect Password";
                        break;

                    case AuthError.InvalidEmail:
                        output = "Invalid Email";
                        break;
     
                }
                loginInvalidOutputText.text = output;
            }
            else
            {
                ClearOutput();
                Debug.Log($"Login Successfull: username- {user.DisplayName} and email- {_email}");
                loginValidOutputText.text = "Login Successfull";
                yield return new WaitForSeconds(1f);
                GameManager.instance.ChangeScene(1); 
                
            }
        }

        
    }

    private IEnumerator RegisterLogic(string _username, string _email, string _password, string _confirmPassword)
    {
        ClearOutput();

        if (_username == "")
        {
            registerInvalidOutputText.text = "Please Enter Username!";
        } 
        else if(_email == "")
        {
            registerInvalidOutputText.text = "Please Enter Email!";
        }
        else if (_password == "")
        {
            registerInvalidOutputText.text = "Please Enter Password!";
        }
        else if (_confirmPassword == "")
        {
            registerInvalidOutputText.text = "Please Enter Confirm Password!";
        }
        else if (_password != _confirmPassword)
        {
            registerInvalidOutputText.text = "Password Does Not Match!";
        }
        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);

            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);

            if (registerTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)registerTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "Unknown Error, Please Try Again";

                switch (error)
                {
                    case AuthError.EmailAlreadyInUse:
                        output = "Email Already In Use!";
                        break;

                    case AuthError.InvalidEmail:
                        output = "Invalid Email!";
                        break;

                    case AuthError.UnverifiedEmail:
                        output = "Email not Verified";
                        break;

                    case AuthError.MissingEmail:
                        output = "Please Enter Your Email";
                        break;

                    case AuthError.MissingPassword:
                        output = "Please Enter Your Password";
                        break;
                }
                registerInvalidOutputText.text = output;
            }
            else
            {
                UserProfile profile = new UserProfile
                {
                    DisplayName = _username,

                };

                var defaultUserTask = user.UpdateUserProfileAsync(profile);

                yield return new WaitUntil(predicate: () => defaultUserTask.IsCompleted);

                if (defaultUserTask.Exception != null)
                {
                    user.DeleteAsync();
                    FirebaseException firebaseException = (FirebaseException)defaultUserTask.Exception.GetBaseException();
                    AuthError error = (AuthError)firebaseException.ErrorCode;
                    string output = "Unknown Error, Please Try Again";

                    switch (error)
                    {
                        case AuthError.Cancelled:
                            output = "Update User Cancelled!";
                            break;

                        case AuthError.SessionExpired:
                            output = "Session Expired!";
                            break;

                        case AuthError.UnverifiedEmail:
                            output = "Email not Verified";
                            break;

                        case AuthError.EmailAlreadyInUse:
                            output = "Email Already In Use!";
                            break;

                    }
                    registerInvalidOutputText.text = output;
                }
                else
                {
                    ClearOutput();
                    Debug.Log($"Firebase User Created Successfully: {user.DisplayName} ({user.UserId})");
                    registerValidOutputText.text = "Account Created";
                    StartCoroutine(SendVerificationEmail());
                }
            }
        }
    }

    private IEnumerator SendVerificationEmail()
    {
        if(user != null)
        {
            var emailTask = user.SendEmailVerificationAsync();

            yield return new WaitUntil(predicate: () => emailTask.IsCompleted);

            if(emailTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)emailTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;

                string output = "Unknown Error, Try Again!";

                switch(error)
                {
                    case AuthError.Cancelled:
                        output = "Verification Task was Cancelled!";
                        break;

                    case AuthError.InvalidRecipientEmail:
                        output = "Invalid Email!";
                        break;

                    case AuthError.TooManyRequests:
                        output = "Too many Requests!";
                        break;
                }

                AuthUIManager.instance.AwaitVerification(false, user.Email, output);
            }
            else
            {
                AuthUIManager.instance.AwaitVerification(true, user.Email, null);
                Debug.Log($"Email Send Successfully");
            }
        }
    }

    private IEnumerator ForgotPassword(string _forgotEmail)
    {
        if (_forgotEmail == "")
        {
            forgotPassEmailText.text = "Please Enter Your Email!";
        } else
        {
            var fetchEmail = auth.FetchProvidersForEmailAsync(_forgotEmail);

            yield return new WaitUntil(predicate: () => fetchEmail.IsCompleted);

            if (fetchEmail.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException)fetchEmail.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;

                string output = "Unknown Error, Try Again!";

                switch (error)
                {
                    case AuthError.InvalidRecipientEmail:
                        output = "Email does not Exist in Database!";
                        break;

                    case AuthError.InvalidEmail:
                        output = "Invalid Email!";
                        break;

                    case AuthError.MissingEmail:
                        output = "Please Enter Your Email";
                        break;

                }
            } else
            {
                var forgotPassTask = auth.SendPasswordResetEmailAsync(_forgotEmail);

                yield return new WaitUntil(predicate: () => forgotPassTask.IsCompleted);

                if (forgotPassTask.Exception != null)
                {
                    FirebaseException firebaseException = (FirebaseException)forgotPassTask.Exception.GetBaseException();
                    AuthError error = (AuthError)firebaseException.ErrorCode;
                    string output = "Unknown Error, Please Try Again";

                    switch (error)
                    {
                        case AuthError.Cancelled:
                            output = "Verification Task was Cancelled!";
                            break;

                        case AuthError.InvalidRecipientEmail:
                            output = "Invalid Email!";
                            break;

                        case AuthError.TooManyRequests:
                            output = "Too many Requests!";
                            break;
                    }
                }
                else
                {
                    forgotPassEmailText.text = ($"Send Email!\nPlease Verify: {_forgotEmail}");
                }
            }
        } 
        
    }

    public void LogOut()
    {
        if(auth.CurrentUser != null)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                auth.SignOut();
                user.DeleteAsync();

                Debug.Log("Signed Out");
            }

        }

        GameManager.instance.ChangeScene(0);
    }
}
