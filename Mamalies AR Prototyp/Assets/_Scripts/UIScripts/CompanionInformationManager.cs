using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CompanionInformationManager : MonoBehaviour
{
    private string doShowCompanionInformationKey = "doShowCompanionInformation";
    private string doShowStartupTutorialKey = "doShowStartupTutorial";

    [Tooltip("The scene that will be loaded after this scene")]
    [SerializeField]
    public string mainMenuSceneName;

    [SerializeField]
    private bool tutorialPage;

    /// <summary>
    /// Checks if player prefers to not see the companion book information again
    /// </summary>
    void Start()
    {
        if (!tutorialPage)
        {
            if (!PlayerPrefs.HasKey(doShowCompanionInformationKey))
            {
                PlayerPrefs.SetInt(doShowCompanionInformationKey, 1);
            }
            else
            {
                if (PlayerPrefs.GetInt(doShowCompanionInformationKey) == 0)
                {
                    //SceneManager.LoadScene(mainMenuSceneName);
                    SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Single);
                }
            }
        }
        else
        {
            if (!PlayerPrefs.HasKey(doShowStartupTutorialKey))
            {
                PlayerPrefs.SetInt(doShowStartupTutorialKey, 1);
            }
            else
            {
                if (PlayerPrefs.GetInt(doShowStartupTutorialKey) == 0)
                {
                    //SceneManager.LoadScene(mainMenuSceneName);
                    SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Single);
                }
            }
        }
    }

    public void LoadMainMenu()
    {
        //SceneManager.LoadScene(mainMenuSceneName);
        SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Single);
    }

    
    public void SetCompanionMessagePlayerPrefs(bool dontShowAgain)
    {
        if (!tutorialPage)
        {
            if (dontShowAgain)
            {
                PlayerPrefs.SetInt(doShowCompanionInformationKey, 0);
            }
            else
            {
                PlayerPrefs.SetInt(doShowCompanionInformationKey, 1);
            }
        }
        else
        {
            if (dontShowAgain)
            {
                PlayerPrefs.SetInt(doShowStartupTutorialKey, 0);
            }
            else
            {
                PlayerPrefs.SetInt(doShowStartupTutorialKey, 1);
            }
        }
    }
}
