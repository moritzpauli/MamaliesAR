using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class LanguageSelection : MonoBehaviour
{
    //warning, messages strings
    [SerializeField]
    [TextArea]
    private string sizeWarningString;
    [SerializeField]
    [TextArea]
    private string noConnectionWarningString;
    [SerializeField]
    [TextArea]
    private string downloadMessageString;
    [SerializeField]
    [TextArea]
    private string deletionMessageString;
    [SerializeField]
    [TextArea]
    private string cancelDownloadMessageString;

    //warning, messages objects
    [SerializeField]
    private GameObject sizeWarningObject;
    [SerializeField]
    private GameObject noConnectionWarningObject;
    [SerializeField]
    private GameObject downloadMessageObject;
    [SerializeField]
    private GameObject deletionMessageObject;
    [SerializeField]
    private GameObject cancelDownloadMessageObject;


    private PersistentLanguageManager persistentLanguageManager;
    private List<TextMeshProUGUI> buttonTexts = new List<TextMeshProUGUI>();

    private string selectedLanguageOne;
    private string selectedLanguageTwo;

    private string buttonPressedLanguage;

    [SerializeField]
    private LanguageAddressablesManager languageAddressablesManager;

    // buttons for every language
    [SerializeField]
    private LanguageButton[] languageButtons;


    [SerializeField]
    private Color cachedTextColor;
    [SerializeField]
    private Color remoteTextColor;
    [SerializeField]
    private string recognitionSceneName;

    [SerializeField]
    private GrayOutButton startButtonGrayout;

    [SerializeField]
    private GameObject loadingPanel;
    private bool loadingLanguageStatus = false;

    bool setupLanguageLoaded = false;

    void Start()
    {

        for (int i = 0; i < languageButtons.Length; i++)
        {
            buttonTexts.Add(languageButtons[i].transform.GetChild(1).GetComponent<TextMeshProUGUI>());
        }

    }




    private void Awake()
    {
        StartCoroutine(UpdateButtonCached());
        StartCoroutine(LoadLanguagePrefs());
        StartCoroutine(DisplayLoadingScreen());
    }

    private IEnumerator DisplayLoadingScreen()
    {
        while (loadingLanguageStatus)
        {
            if (!loadingPanel.activeSelf)
            {
                loadingPanel.SetActive(true);
            }
            yield return null;
        }
        loadingPanel.SetActive(false);

    }

    /// <summary>
    /// Loads the currently selected language preferences and updates buttons/ german toggle box
    /// </summary>
    private IEnumerator LoadLanguagePrefs()
    {
        persistentLanguageManager = GameObject.FindObjectOfType<PersistentLanguageManager>();
        print("Language One Set:" + persistentLanguageManager.GetLanguageOne());
        print("Language Two Set:" + persistentLanguageManager.GetLanguageTwo());
        //print("Deutsch?? " + persistentLanguageManager.GetPlayGerman());
        if (persistentLanguageManager.GetLanguageOne() != null && persistentLanguageManager.GetLanguageOne() != "")
        {
            CoroutineReturnData getLanguageSize = new CoroutineReturnData(this, languageAddressablesManager.GetLanguageSize(persistentLanguageManager.GetLanguageOne()));
            yield return getLanguageSize.coroutine;
            yield return new WaitUntil(() => !getLanguageSize.running);
            if ((long)(getLanguageSize.result) == 0)
            {
                selectedLanguageOne = persistentLanguageManager.GetLanguageOne();
            }
           
            
        }
        //yield return new WaitUntil(() => setupLanguageLoaded);
        if (persistentLanguageManager.GetLanguageTwo() != null && persistentLanguageManager.GetLanguageTwo() != "")
        {
            CoroutineReturnData getLanguageSize = new CoroutineReturnData(this, languageAddressablesManager.GetLanguageSize(persistentLanguageManager.GetLanguageTwo()));
            yield return getLanguageSize.coroutine;
            yield return new WaitUntil(() => !getLanguageSize.running);
            if ((long)(getLanguageSize.result) == 0)
            {
                print("language two will get selected");
                selectedLanguageTwo = persistentLanguageManager.GetLanguageTwo();
            }
        }
        if (persistentLanguageManager.OneLanguageSet())
        {
            startButtonGrayout.ColorIn();
        }
        else
        {
            startButtonGrayout.GrayOut();
        }
        SelectButtonsInOrder(selectedLanguageOne, selectedLanguageTwo);
        //print("Sprache?? " + persistentLanguageManager.GetLanguage());
    }

    /// <summary> 
    /// Updates the Buttons Visuals to mark the currently selected language
    /// </summary>
    public IEnumerator UpdateButtonSelect(string languagePressed)
    {
        buttonPressedLanguage = languagePressed;

        CoroutineReturnData getLanguageSize = new CoroutineReturnData(this, languageAddressablesManager.GetLanguageSize(languagePressed));
        yield return getLanguageSize.coroutine;
        yield return new WaitUntil(() => !getLanguageSize.running);
        if ((long)(getLanguageSize.result) == 0)
        {
            if (selectedLanguageOne == null)
            {
                selectedLanguageOne = languagePressed;
            }
            else if (selectedLanguageTwo == null)
            {
                selectedLanguageTwo = languagePressed;
            }
            else
            {
                selectedLanguageOne = selectedLanguageTwo;
                selectedLanguageTwo = languagePressed;
            }


            SelectButtonsInOrder(selectedLanguageOne, selectedLanguageTwo);
            persistentLanguageManager.SetLanguageOne(selectedLanguageOne);
            persistentLanguageManager.SetLanguageTwo(selectedLanguageTwo);
            if (persistentLanguageManager.OneLanguageSet())
            {
                startButtonGrayout.ColorIn();
            }
            else
            {
                startButtonGrayout.GrayOut();
            }
        }
        else
        {
            DisplayDownloadMessage((long)getLanguageSize.result);
        }
    }

    private void SelectButtonsInOrder(string firstLanguage, string secondLanguage)
    {
        foreach (LanguageButton button in languageButtons)
        {

            if (firstLanguage != null && button.name.ToLower() == firstLanguage)
            {
                button.SelectButton(true);
            }
            else if (secondLanguage != null && button.name.ToLower() == secondLanguage)
            {
                button.SelectButton(false);
            }
            else
            {
                button.DeselectButton();
            }

        }
    }

    /// <summary>
    /// Updates the buttons visuals to mark which languages are currently downloaded
    /// </summary>
    private IEnumerator UpdateButtonCached()
    {
        loadingLanguageStatus = true;
        for (int i = 0; i < languageButtons.Length; i++)
        {
            CoroutineReturnData isLanguageCached = new CoroutineReturnData(this, languageAddressablesManager.IsLanguageCached(languageButtons[i].name));
            yield return new WaitUntil(() => !isLanguageCached.running);
            yield return isLanguageCached.coroutine;
            //print(languageButtons[i].name + " " + isLanguageCached.result);
            if ((bool)(isLanguageCached.result))
            {
                buttonTexts[i].color = cachedTextColor;
                //print("set button color downloaded    GREEN");
            }
            else
            {
                buttonTexts[i].color = remoteTextColor;
                languageButtons[i].DeselectButton();
                //print("set button color NOT downloaded GRAY");
            }
        }
        loadingLanguageStatus = false;
    }


    #region Hide Display UI Windows

    public void DisplayDeletionMessage()
    {
        deletionMessageObject.SetActive(true);
        deletionMessageObject.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = deletionMessageString;
    }

    public void HideDeletionMessage()
    {
        deletionMessageObject.SetActive(false);
    }

    private void HideDownloadMessage()
    {
        downloadMessageObject.SetActive(false);
    }

    public void HideCancelDownloadMessage()
    {
        cancelDownloadMessageObject.SetActive(false);
    }

    public void DisplaySizeWarning(double languageSize, double freeCacheSize)
    {
        sizeWarningObject.SetActive(true);
        string currentSizeWarning = sizeWarningString;
        float languageSizeConverted = ConvertSizeToMb(languageSize);
        float freeCacheSizeConverted = ConvertSizeToMb(freeCacheSize);
        currentSizeWarning = currentSizeWarning.Replace("#num1#", languageSizeConverted.ToString());
        currentSizeWarning = currentSizeWarning.Replace("#num2#", freeCacheSizeConverted.ToString());
        sizeWarningObject.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = currentSizeWarning;
    }

    public void DisplayConnectivityWarning()
    {
        noConnectionWarningObject.SetActive(true);
        noConnectionWarningObject.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = noConnectionWarningString;
    }

    public void DisplayDownloadMessage(double languageSize)
    {
        downloadMessageObject.SetActive(true);
        string currentDownloadMessage = downloadMessageString;
        float languageSizeConverted = ConvertSizeToMb(languageSize);
        currentDownloadMessage = currentDownloadMessage.Replace("#num1#", languageSizeConverted.ToString());
        downloadMessageObject.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = currentDownloadMessage;

    }

    public void DisplayCancelDownloadMessage()
    {
        if (languageAddressablesManager.IsDownloadRunning())
        {
            cancelDownloadMessageObject.SetActive(true);
            cancelDownloadMessageObject.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>().text = cancelDownloadMessageString;
        }
    }

    #endregion




    /// <summary>
    /// converts a long in byte to a float in megabyte with two decimal places
    /// </summary>
    /// <param name="languageSize"></param>
    /// <returns></returns>
    private float ConvertSizeToMb(double languageSize)
    {
        double languageSizeConverted = languageSize / 1000000;
        languageSizeConverted = Mathf.Round((float)languageSizeConverted * 100) / 100;
        float languageSizefloat = (float)languageSizeConverted;
        return languageSizefloat;
    }

    public void LanguageButtonPressed(string LangId)
    {
        StartCoroutine(UpdateButtonSelect(LangId));
        //print(LangId);
        print("button presssed" + LangId);

    }

    public void DownloadButtonPressed()
    {
        StartCoroutine(languageAddressablesManager.DownloadLanguagePackage(buttonPressedLanguage));
        HideDownloadMessage();

    }

    public void PostDownloadUiUpdate(string LangId)
    {
        StartCoroutine(UpdateButtonCached());
        StartCoroutine(UpdateButtonSelect(LangId));


    }

    public void PostDeletionUiUpdate()
    {
        persistentLanguageManager.SetLanguageOne("");
        persistentLanguageManager.SetLanguageTwo("");
        selectedLanguageOne = "";
        selectedLanguageTwo = "";
        StartCoroutine(UpdateButtonCached());
    }



    public void LoadRecognitionScene()
    {
        if (persistentLanguageManager.OneLanguageSet())
        {

            if (recognitionSceneName != null)
            {
                SceneManager.LoadSceneAsync(recognitionSceneName);
                loadingPanel.SetActive(true);
            }


        }
    }

    //private void SceneLoadCompleted(AsyncOperationHandle<UnityEngine.ResourceManagement.ResourceProviders.SceneInstance> obj)
    //{
    //    if (obj.Status == AsyncOperationStatus.Succeeded)
    //    {
    //        Debug.Log(obj.Result.Scene.name + " successfully loaded.");
    //    }
    //}
}
