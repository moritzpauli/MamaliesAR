using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LanguageButton : MonoBehaviour
{

    public bool selected;


    private GameObject background;
    private GameObject selectedOrder;
    private GameObject selectedNumberOne;
    private GameObject selectedNumberTwo;

    private void Start()
    {
        background = transform.GetChild(0).gameObject;
        selectedOrder = transform.GetChild(2).gameObject;
        selectedNumberOne = transform.GetChild(2).GetChild(0).gameObject;
        selectedNumberTwo = transform.GetChild(2).GetChild(1).gameObject;
    }

    //sets variables and visually selects button
    public void SelectButton(bool first)
    {
        selected = true;


        background.SetActive(true);
        selectedOrder.SetActive(true);

        if (first)
        {
            selectedNumberOne.SetActive(true);
            selectedNumberTwo.SetActive(false);
        }
        else
        {
            selectedNumberOne.SetActive(false);
            selectedNumberTwo.SetActive(true);
        }
    }

    public void DeselectButton()
    {
        selected = false;

        background.SetActive(false);
        selectedOrder.SetActive(false);
    }
}
