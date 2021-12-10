using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchTrackingPlatform : MonoBehaviour
{
    [SerializeField]
    private VignetteRecognitionAndroid androidRecognition;

    [SerializeField]
    private VignetteRecognitionIOS iOSvignetteRecognition;

    private void OnEnable()
    {

#if UNITY_ANDROID
        androidRecognition.enabled = true;
        iOSvignetteRecognition.enabled = false;
#endif

#if UNITY_IOS
androidRecognition.enabled = false;
        iOSvignetteRecognition.enabled = true;
#endif
    }
}
