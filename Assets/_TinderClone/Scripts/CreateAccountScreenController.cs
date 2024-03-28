using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using JsonUtility = UnityEngine.JsonUtility;
using SimpleFileBrowser;
using UnityEngine.SceneManagement;

public class CreateAccountScreenController : MonoBehaviour
{
    //public string serverurl = "http://localhost:3001/";
    const string serverurl = "https://simple-server-4et5.onrender.com/";



    public GameObject signInScreen;
    public GameObject signUpScreen;
    public GameObject createProfileScreen;


    [Header("Sign Up Screen")]
    public TMP_InputField nameInputField;
    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_Dropdown genderDropdown;
    public TMP_Text genderText;
    [Header("Sign In Screen")]
    public TMP_InputField emailInputField2;
    public TMP_InputField passwordInputField2;

    [Header("Create Profile Screen")]
    public TMP_InputField descriptionInputField;
    public RawImage avatarImage;


    public UserData userData;

    private void Start()
    {
        genderDropdown.onValueChanged.AddListener((int value) =>
        {
            //0 = male, 1= female, 2 = les, 3 = gay, 4 = bi, 5 = trans
            switch (value)
            {
                case 0:
                    genderText.text = "Straight Woman";
                    break;
                case 1:
                    genderText.text = "Straight Man";
                    
                    break;
                case 2:
                    genderText.text = "Lesbian";
                    break;
                case 3:
                    genderText.text = "Gay Man";
                    break;
                case 4:
                    genderText.text = "Bisexual";
                    break;
                case 5:
                    genderText.text = "Transgender";
                    break;
            }
        });

        signInScreen.SetActive(false);
        signUpScreen.SetActive(true);
        createProfileScreen.SetActive(false);
    }

    public void OnAlreadyHaveAccountButtonClicked()
    {
        signInScreen.SetActive(true);
        signUpScreen.SetActive(false);
    }
    public void DontHaveAccountButtonClicked()
    {
        signInScreen.SetActive(false);
        signUpScreen.SetActive(true);
    }




    public void OnClickSignInButton()
    {
        StartCoroutine(SignIn());
    }
    IEnumerator SignIn()
    {
        string email = emailInputField2.text;
        string password = passwordInputField2.text;

        //http request to server
        WWWForm form = new WWWForm();
        form.AddField("email", email);
        form.AddField("password", password);

        UnityWebRequest www = UnityWebRequest.Post(serverurl+"signin", form);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            //parse json
            UserData userData = JsonUtility.FromJson<UserData>(www.downloadHandler.text);
            Debug.Log("Name: " + userData.name);
            Debug.Log("Email: " + userData.email);
            Debug.Log("username: " + userData.name);
            Debug.Log("Gender: " + userData.gender.ToString());

            Debug.Log("Go to the main menu ^^ finish the login account screen");
            SceneManager.LoadScene("SwipeScreen");
        }

    }

    public void OnClickSignUpButton()
    {
        StartCoroutine(SignUp());
    }
    IEnumerator SignUp()
    {
        userData = new UserData();
        userData.name = nameInputField.text;
        userData.email = emailInputField.text;
        userData.password = passwordInputField.text;
        userData.gender = (Gender)genderDropdown.value;
        //debugging
        Debug.Log("Name: " + userData.name);
        Debug.Log("Email: " + userData.email);
        Debug.Log("Password: " + userData.password);
        Debug.Log("Gender: " + userData.gender.ToString());


        WWWForm form = new WWWForm();
        form.AddField("email", userData.email);
        form.AddField("password", userData.password);
        form.AddField("name", userData.name);
        form.AddField("gender", (int)userData.gender);

        Debug.Log(serverurl + "signup");

        //http request to server
        UnityWebRequest www = UnityWebRequest.Post(serverurl+"signup", form);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);

        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            //parse json
            UserData userData = JsonUtility.FromJson<UserData>(www.downloadHandler.text);
            Debug.Log("Name: " + userData.name);
            Debug.Log("Email: " + userData.email);
            Debug.Log("username: " + userData.name);

            //then if everything is ok, go to the create profile screen
            signInScreen.SetActive(false);
            signUpScreen.SetActive(false);
            createProfileScreen.SetActive(true);
        }

    }





    //about the profile screen

    public void OnClickEditAvatarButton()
    {
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"), new FileBrowser.Filter("Text Files", ".txt", ".pdf"));

        FileBrowser.ShowLoadDialog((paths) =>
        {
            StartCoroutine(LoadImage(paths[0]));
        }, null, FileBrowser.PickMode.Files, false, null, null, "Select Image", "Select");

    }
    
    IEnumerator LoadImage(string path)
    {

        Debug.Log("Loading image from: " + path);
        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails
        Texture2D texture = null;
        byte[] fileData;

        if (System.IO.File.Exists(path))
        {
            fileData = System.IO.File.ReadAllBytes(path);
            texture = new Texture2D(2, 2);
            texture.LoadImage(fileData); //..this will auto-resize the texture dimensions.
        }

        //assign the texture to the raw image
        avatarImage.texture = texture;

        //upload the image to the server
        yield return UploadImage(texture);
    }

    IEnumerator UploadImage(Texture2D texture)
    {
        //convert the texture to a byte array
        byte[] bytes = texture.EncodeToPNG();

        string fileName = "avatar + " + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
        //create a form
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", bytes, fileName, "image/png");

        //send the form to the server
        UnityWebRequest www = UnityWebRequest.Post(serverurl + "uploadimage", form);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            //parse json
            string imageUrl = www.downloadHandler.text;
            Debug.Log("Image URL: " + imageUrl);

            //save the image url to the user data
            userData.avatarUrl = imageUrl;
        }
    }

    public void OnClickSaveProfileButton()
    {
        StartCoroutine(SaveProfile());
    }
    IEnumerator SaveProfile()
    {
        userData.description = descriptionInputField.text;

        //create a form
        WWWForm form = new WWWForm();
        form.AddField("email", userData.email);
        form.AddField("description", userData.description);
        form.AddField("avatarUrl", userData.avatarUrl);

        //send the form to the server
        UnityWebRequest www = UnityWebRequest.Post(serverurl + "updateprofile", form);

        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            //parse json
            string response = www.downloadHandler.text;
            //the response should be the entire user data
            Debug.Log("User Data: " + response);
            //then go to the main menu
            Debug.Log("Go to the main menu ^^ finish the create account screen");
            SceneManager.LoadScene("SwipeScreen");

        }
    }
}
