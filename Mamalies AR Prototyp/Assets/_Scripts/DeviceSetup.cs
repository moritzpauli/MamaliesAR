using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class DeviceSetup : MonoBehaviour
{
   
    /// <summary>
    /// Set rudimentary Device parameters
    /// </summary>
    void Start()
    {         
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.orientation = ScreenOrientation.Portrait;        
    }

   
}
