using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CompanionInformationManager : MonoBehaviour
{
    private string doShowCompanionInformationKey = "doShowCompanionInformation";

    [Tooltip("The scene that will be loaded after this scene")]
    [SerializeField]
    public string mainMenuSceneName;

    /// <summary>
    /// Checks if player prefers to not see the companion book information again
    /// </summary>
    void Start()
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

    public void LoadMainMenu()
    {
        //SceneManager.LoadScene(mainMenuSceneName);
        SceneManager.LoadSceneAsync(mainMenuSceneName, LoadSceneMode.Single);
    }

    
    public void SetCompanionMessagePlayerPrefs(bool dontShowAgain)
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
}
