using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractiveUiDeactivator : MonoBehaviour
{
    [SerializeField]
    private Button[] languageButtons;

    [SerializeField]
    private Button[] otherUiButtons;



    //[SerializeField]
    //private Scrollbar languageScroll;


    /// <summary>
    /// deactivets interactive component of ui
    /// </summary>
    public void DeactivateInteractiveUi()
    {
        foreach (Button button in languageButtons)
        {
            button.enabled = false;
            button.GetComponent<Image>().enabled = false;
        }

        foreach (Button button in otherUiButtons)
        {
            button.enabled = false;
           
        }

        //languageScroll.enabled = false;
    }


    /// <summary>
    /// activets interactive component of ui
    /// </summary>
    public void ActivateInteractiveUi()
    {

        foreach (Button button in languageButtons)
        {
            button.enabled = true;
            button.GetComponent<Image>().enabled = true;
        }

        foreach (Button button in otherUiButtons)
        {
            button.enabled = true;
        }

       // languageScroll.enabled = true;
    }
}
