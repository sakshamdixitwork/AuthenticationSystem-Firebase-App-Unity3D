using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Facebook.Unity;

public class FacebookManager : MonoBehaviour
{
    public static FacebookManager instance;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }

        FB.Init(SetInit, onHideUnity);
    }

    void SetInit()
    {
        if(FB.IsLoggedIn)
        {
            Debug.Log("Logged in Sucessfully!");
        } else
        {
            Debug.Log("Login Failed!");
        }
    }

    void onHideUnity(bool isGameShow)
    {
        if(isGameShow)
        {
            Time.timeScale = 1;
        } else
        {
            Time.timeScale = 0;
        }
    }

    public void FBLogin()
    {
        List<string> permissions = new List<string>();
        permissions.Add("public_profile");
        permissions.Add("email");
        FB.LogInWithReadPermissions(permissions, AuthCallBack);
    }

    void AuthCallBack(ILoginResult result)
    {
        if(FB.IsLoggedIn)
         {
             var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
             Debug.Log(aToken.UserId + " Logged in Successfully");

        }
        else
        {
            Debug.Log("Login Failed!");
        }
        
    }
}
