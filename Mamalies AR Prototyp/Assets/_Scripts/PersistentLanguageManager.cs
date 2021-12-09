using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Carry selected language information through scenes
/// </summary>
public class PersistentLanguageManager : MonoBehaviour
{
    private string selectedLanguageOne;
    private string selectedLanguageTwo;


    private const string selectedLanguageOneKey = "selectedLanguageOne";
    private const string selectedLanguageTwoKey = "selectedLanguageTwo";

    private void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
        print(selectedLanguageOne + selectedLanguageTwo);
        //get language and playgerman from Player Preferences
        if (PlayerPrefs.HasKey(selectedLanguageOneKey))
        {
            selectedLanguageOne = PlayerPrefs.GetString(selectedLanguageOneKey);
        }
        else
        {
            selectedLanguageOne = "";
        }


        if (PlayerPrefs.HasKey(selectedLanguageTwoKey))
        {
            selectedLanguageTwo = PlayerPrefs.GetString(selectedLanguageTwoKey);
        }
        else
        {
            selectedLanguageTwo = "";
        }
    }


    public void SetLanguageOne(string lang)
    {
        selectedLanguageOne = lang;
        PlayerPrefs.SetString(selectedLanguageOneKey, lang);
    }

    public void SetLanguageTwo(string lang)
    {
        selectedLanguageTwo = lang;
        PlayerPrefs.SetString(selectedLanguageTwoKey, lang);
    }

    public string GetLanguageOne()
    {

        return selectedLanguageOne;

    }

    public string GetLanguageTwo()
    {
        return selectedLanguageTwo;
    }

    public bool BothLanguagesSet()
    {
        if (selectedLanguageOne != null && selectedLanguageOne != "" && selectedLanguageTwo != null && selectedLanguageTwo != "")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool OneLanguageSet()
    {
        if ((selectedLanguageOne != null && selectedLanguageOne != "") || (selectedLanguageTwo != null && selectedLanguageTwo != ""))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
