using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AuthUIManager : MonoBehaviour
{
    public static AuthUIManager instance;

    [Header("References")]
    [SerializeField]
    private GameObject checkingForAccountUI;
    [SerializeField]
    private GameObject mainMenuUI;
    [SerializeField]
    private GameObject loginUI;
    [SerializeField]
    private GameObject registerUI;
    [SerializeField]
    private GameObject verifyEmailUI;
    [SerializeField]
    private GameObject forgotPasswordUI;
    [SerializeField]
    private TMP_Text verifyEmailText;
    [SerializeField]
    private TMP_Text verifyForgotPassText;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else if(instance != this)
        {
            Destroy(gameObject);
        }

        ClearUI();
    }

    private void Start()
    {
        
        mainMenuUI.SetActive(true);
    }

    private void ClearUI()
    {
        mainMenuUI.SetActive(false);
        registerUI.SetActive(false);
        loginUI.SetActive(false);
        forgotPasswordUI.SetActive(false);
        verifyEmailUI.SetActive(false);
        FirebaseManager.instance.ClearOutput();
    }

    public void RegisterUI()
    {
        ClearUI();
        registerUI.SetActive(true);
    }

    public void LoginUI()
    {
        ClearUI();
        loginUI.SetActive(true);
    }

    public void ForgotPasswordUI()
    {
        ClearUI();
        forgotPasswordUI.SetActive(true);

    }

    public void BackButton()
    {
        ClearUI();
        mainMenuUI.SetActive(true);
    }

    public void AwaitVerification(bool _emailSend, string _email, string output)
    {
        ClearUI();
        verifyEmailUI.SetActive(true);

        if(_emailSend)
        {
            verifyEmailText.text = ($"Send Email!\nPlease Verify: {_email}");
        }
        else
        {
            verifyEmailText.text = ($"Email not Sent: {output}\nPlease Verify: {_email}");
        }
    }

}
