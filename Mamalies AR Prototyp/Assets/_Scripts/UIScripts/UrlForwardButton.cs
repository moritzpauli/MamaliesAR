using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrlForwardButton : MonoBehaviour
{
    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }
}
