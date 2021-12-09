using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GrayOutButton : MonoBehaviour
{
    [SerializeField]
    private bool grayedOut = true;

    [SerializeField]
    private Sprite graySprite;
    [SerializeField]
    private Sprite colorSprite;

    private Image image;
    private Button button;

    private void Start()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();

        if (grayedOut)
        {
            GrayOut();
        }
        else
        {
            ColorIn();
        }
    }

    public void GrayOut()
    {
        image.sprite = graySprite;
        button.enabled = false;
    }

    public void ColorIn()
    {
        image.sprite = colorSprite;
        button.enabled = true;
    }
}
