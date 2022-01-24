using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartupTutorialManager : MonoBehaviour
{

    [SerializeField]
    private LanguageButton[] tutorialLanguageButtons;

    [SerializeField]
    private GameObject[] tutorialTextFields;

    [SerializeField]
    private ScrollRect tutorialScroll;

    private void Start()
    {
        string startSelected = "";
        foreach(LanguageButton button in tutorialLanguageButtons)
        {
            if (button.selected)
            {
                startSelected = button.name;
                break;
            }
        }

        foreach(GameObject go in tutorialTextFields)
        {
            if(go.name == startSelected)
            {
                go.SetActive(true);
                break;
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
            if (button.name != lang)
            {
                button.DeselectButton();
            }
            else
            {
                button.SelectButton(true);
            }
        }

        foreach (GameObject go in tutorialTextFields)
        {
            if (go.name != lang)
            {
                go.SetActive(false);
            }
            else
            {
                go.SetActive(true);
            }
        }

        //TODO-TEST correctly recalculate the scroll size
        tutorialScroll.GraphicUpdateComplete();
    }
}
