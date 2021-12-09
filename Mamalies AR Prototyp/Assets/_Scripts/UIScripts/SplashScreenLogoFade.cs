using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


/// <summary>
/// Zooms into the logo fades it and loads next scene in build index
/// </summary>
public class SplashScreenLogoFade : MonoBehaviour
{
    [SerializeField]
    private Image splashScreenIcon;

    [SerializeField]
    private float zoomSpeed;

    [SerializeField]
    private float fadeSpeed;

    //serialized scene names
    [SerializeField]
    private string companionInfoSceneName = "02 CompanionBookInformation_Mamalies";
    [SerializeField]
    private string mainMenuSceneName = "03 MainMenu_Mamalies";
    [SerializeField]
    private string trackingSceneName = "04 ImageRecognition_Mamalies";

    [Tooltip("The time it takes to load a new scene when no splash screen Animation is selected")]
    [SerializeField]
    private float fallbackTransationTime;

    private float fallbackTimer = 0;

    private string doShowCompanionInformationKey = "doShowCompanionInformation";

    // debug
    [SerializeField]
    private bool loadTracking;
  



    void Start()
    {
        if (loadTracking)
        {
            SceneManager.LoadScene(trackingSceneName);
        }
    }

    /// <summary>
    /// enlarge and fade out logo, then load scene
    /// </summary>
    void Update()
    {
        if (splashScreenIcon != null)
        {
            if (splashScreenIcon.rectTransform.localScale.x < 2.1f)
            {
                splashScreenIcon.rectTransform.localScale =
                    new Vector3(splashScreenIcon.rectTransform.localScale.x + zoomSpeed * Time.deltaTime,
                    splashScreenIcon.rectTransform.localScale.y + zoomSpeed * Time.deltaTime,
                    splashScreenIcon.rectTransform.localScale.z + zoomSpeed * Time.deltaTime);
            }

            if (splashScreenIcon.rectTransform.localScale.x > 1.6f)
            {
                splashScreenIcon.color = new Color(splashScreenIcon.color.r, splashScreenIcon.color.g,
                    splashScreenIcon.color.b, splashScreenIcon.color.a - Time.deltaTime * fadeSpeed);
            }

            if (splashScreenIcon.color.a <= 0)
            {
                LoadNextScene();
            }
        }
        else
        {
            fallbackTimer += Time.deltaTime;
            if (fallbackTimer >= fallbackTransationTime)
            {
                LoadNextScene();
            }
        }
    }

    /// <summary>
    /// Either loads the companion info scene or main menu
    /// </summary>
    private void LoadNextScene()
    {
        if (!PlayerPrefs.HasKey(doShowCompanionInformationKey) || PlayerPrefs.GetInt(doShowCompanionInformationKey) == 1)
        {
            SceneManager.LoadScene(companionInfoSceneName);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
