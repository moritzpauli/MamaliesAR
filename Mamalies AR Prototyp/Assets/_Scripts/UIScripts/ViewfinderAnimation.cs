using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ViewfinderAnimation : MonoBehaviour
{
    [SerializeField]
    private Image viewfinder;

    [SerializeField]
    private Animator uiAnimator;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartViewfinderAnimation();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            StopViewfinderAnimation();
        }
    }
    public void StartViewfinderAnimation()
    {     
            uiAnimator.SetBool("PlayAnimation",true);
    }

    public void StopViewfinderAnimation()
    {
        uiAnimator.SetBool("PlayAnimation", false);
    }

    public void DeActivateViewfinder()
    {
        if (viewfinder.enabled)
        {
            viewfinder.enabled = false;
        }
        else
        {
            viewfinder.enabled = true;
        }
    }
}
