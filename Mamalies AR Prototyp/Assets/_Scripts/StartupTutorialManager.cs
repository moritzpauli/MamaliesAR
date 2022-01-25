using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartupTutorialManager : MonoBehaviour
{

    [SerializeField]
    private LanguageButton[] tutorialLanguageButtons;

    [SerializeField]
    private GameObject[] tutorialTexts;

    [SerializeField]
    private ScrollRect tutorialScroll;

    [SerializeField]
    private Canvas canvas;



    private void Start()
    {
        string startSelected = "";
        foreach(LanguageButton button in tutorialLanguageButtons)
        {
            if (button.selected)
            {
                startSelected = button.gameObject.name.ToLower();
                break;
            }
        }

        foreach(GameObject go in tutorialTexts)
        {
            if(go.name.ToLower() == startSelected)
            {
                go.SetActive(true);
                tutorialScroll.content = go.GetComponent<RectTransform>();
                
            }
            else
            {
                go.SetActive(false);
            }
        }
    }



    /// <summary>
    /// De/Selects the buttons and de/activates the corresponding textfields containing the tutorial
    /// </summary>
    /// <param name="lang"></param>
    public void TutorialButtonPressed(string lang)
    {
        foreach (LanguageButton button in tutorialLanguageButtons)
        {
            if (button.gameObject.name.ToLower() != lang)
            {
                button.DeselectButton();
            }
            else
            {
                button.SelectButton(true);
            }
        }

        foreach (GameObject go in tutorialTexts)
        {
            if (go.name.ToLower() != lang)
            {
                go.SetActive(false);
            }
            else
            {
                go.SetActive(true);
                tutorialScroll.content = go.GetComponent<RectTransform>();
            }
        }

       // tutorialScroll.Rebuild(CanvasUpdate.LatePreRender);
    }
}
