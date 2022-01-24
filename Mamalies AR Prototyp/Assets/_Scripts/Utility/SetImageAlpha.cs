using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetImageAlpha : MonoBehaviour
{
    private Image attachedImage;

    private void Start()
    {
        if (GetComponent<Image>())
        {
            attachedImage = GetComponent<Image>();
        }
    }

    /// <summary>
    /// Sets the alpha of the gameobjects image component, usable by eg slider
    /// </summary>
    /// <param name="newAlpha"></param>
    public void SetAttachedImageAlpha(float newAlpha)
    {
        if (attachedImage)
        {
            attachedImage.color = new Color(attachedImage.color.r, attachedImage.color.g, attachedImage.color.b, newAlpha);
        }
    }
}
