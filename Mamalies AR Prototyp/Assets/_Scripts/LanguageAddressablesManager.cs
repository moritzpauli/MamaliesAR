using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Networking;
using System.Runtime.InteropServices;

public class LanguageAddressablesManager : MonoBehaviour
{
    [SerializeField]
    private LanguageSelection languageSelection;
    [SerializeField]
    private Image progressBar;
    [SerializeField]
    private Image spinner;
    [SerializeField]
    private TextMeshProUGUI progressPercentageText;
    [SerializeField]
    private GameObject downloadUiMask;

    private InteractiveUiDeactivator uiDeactivator;

    private AsyncOperationHandle<long> sizeCheckHandle;
    private AsyncOperationHandle languageDownloadHandle;


    private string currentLanguage;

    private long freeDriveSpace;

    private bool abortProcessRunning;
    private bool downloadRunning;

    private bool currentDownloadAborted = false;


#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int HWUtils_getFreeMemory();
#endif

    private void Start()
    {
        uiDeactivator = GetComponent<InteractiveUiDeactivator>();
    }

    /// <summary>
    /// Checks if the language is already cached(downloaded and saved) on Device
    /// </summary>
    /// <param name="lan"></param>
    /// <returns></returns>
    public IEnumerator IsLanguageCached(string lan)
    {
        lan = lan.ToLower();
        AssetLabelReference language = new AssetLabelReference();
        language.labelString = lan;
        sizeCheckHandle = Addressables.GetDownloadSizeAsync(language);
        yield return new WaitUntil(() => sizeCheckHandle.IsDone);
        print(lan + " " + sizeCheckHandle.Result);
        if (sizeCheckHandle.Result == 0L)
        {
            yield return true;
        }
        else
        {
            yield return false;
        }

    }

    /// <summary>
    /// Get the download size of the Language package
    /// </summary>
    /// <param name="lan"></param>
    /// <returns></returns>
    public IEnumerator GetLanguageSize(string lan)
    {
        lan = lan.ToLower();
        AssetLabelReference language = new AssetLabelReference();
        language.labelString = lan;
        sizeCheckHandle = Addressables.GetDownloadSizeAsync(language);
        yield return new WaitUntil(() => sizeCheckHandle.IsDone);
        yield return sizeCheckHandle.Result;

    }

#if UNITY_IOS && !UNITY_EDITOR


    public static int GetFreeMemoryIOS()
    {
        print("IOS Device Diskspace available: " + HWUtils_getFreeMemory());
        return HWUtils_getFreeMemory();
    }
#endif

    /// <summary>
    /// Download Addressables of specified tag
    /// </summary>
    /// <param name="lan"></param>
    /// <returns></returns>
    public IEnumerator DownloadLanguagePackage(string lan)
    {
        uiDeactivator.DeactivateInteractiveUi();
        downloadUiMask.SetActive(true);
        downloadRunning = true;
        currentDownloadAborted = false;

        print("downloading " + lan);
        lan = lan.ToLower();
        AssetLabelReference language = new AssetLabelReference();
        language.labelString = lan;

        languageDownloadHandle = Addressables.DownloadDependenciesAsync(language, true);
        languageDownloadHandle.Completed += PackageDownloadComplete;
        currentLanguage = lan;

        while (!languageDownloadHandle.IsDone)
        {
            DownloadStatus status = languageDownloadHandle.GetDownloadStatus();
            int progress = (int)(status.Percent * 100);
            if (progress <= 0)
            {
                progress = 1;
            }
            progressPercentageText.text = (progress - 1f).ToString() + " %";
            progressBar.fillAmount = status.Percent - 0.01f;
            yield return null;
            spinner.rectTransform.Rotate(new Vector3(0, 0, 300 * Time.deltaTime));

        }

    }


    /// <summary>
    /// Update UI, if Download failed call CheckFailureCause()
    /// </summary>
    /// <param name="handle"></param>
    private void PackageDownloadComplete(AsyncOperationHandle handle)
    {
        downloadRunning = false;
        uiDeactivator.ActivateInteractiveUi();
        downloadUiMask.SetActive(false);
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            progressPercentageText.text = "Download completed";
            progressBar.fillAmount = 1f;
            languageSelection.PostDownloadUiUpdate(currentLanguage);

        }
        else
        {
            StartCoroutine(CheckFailureCause(currentLanguage));
        }
        //Addressables.Release(handle);
    }


    /// <summary>
    /// Check if there is an internet connection or if the device has lacking storage
    /// </summary>
    /// <param name="lan"></param>
    /// <returns></returns>
    private IEnumerator CheckFailureCause(string lan)
    {
        if (!currentDownloadAborted)
        {
            //ping google to check for connection
            UnityWebRequest googleCheck = new UnityWebRequest("http://google.com");
            yield return googleCheck.SendWebRequest();
            if (googleCheck.error != null)
            {
                languageSelection.DisplayConnectivityWarning();
            }


            //check if theres enough drive space
            long languageSize;
            CoroutineReturnData getLanguageSize = new CoroutineReturnData(this, GetLanguageSize(lan));
            yield return getLanguageSize.coroutine;
            yield return new WaitUntil(() => !getLanguageSize.running);
            languageSize = (long)getLanguageSize.result;

            freeDriveSpace = 0;

#if !UNITY_IOS
            DriveInfo[] allDrives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in allDrives)
            {
                if (drive.IsReady)
                {
                    if (drive.AvailableFreeSpace > freeDriveSpace && drive.DriveType != DriveType.Fixed)
                    {
                        freeDriveSpace = drive.AvailableFreeSpace;
                        print(drive.Name);
                        print(drive.DriveType);
                    }
                }
            }

#endif


#if UNITY_IOS && !UNITY_EDITOR
        freeDriveSpace = GetFreeMemoryIOS();
#endif



            print("language size" + languageSize / 1000000);
            print("free drive space" + freeDriveSpace / 1000000);

            if (languageSize > freeDriveSpace)
            {
                languageSelection.DisplaySizeWarning(languageSize, freeDriveSpace);

            }
            print("checked");
        }
    }


    // Debug
    /// <summary>
    /// Deletes all cached language packages
    /// </summary>
    public void DeleteCachedLanguages()
    {
        Caching.ClearCache();
        languageSelection.PostDeletionUiUpdate();
        languageSelection.HideDeletionMessage();
    }


    /// <summary>
    /// Starts AbortAllWebRequests(), called by button
    /// </summary>
    public void AbortWebRequestsButton()
    {
        if (!abortProcessRunning && downloadRunning)
        {
            currentDownloadAborted = true;
            StartCoroutine(AbortAllWebRequests());
            uiDeactivator.ActivateInteractiveUi();
            downloadUiMask.SetActive(false);
        }
        languageSelection.HideCancelDownloadMessage();
    }


    /// <summary>
    /// Aborts all current webrequests, in this case Language Download, updates progress display ui, clears cache of aborted download package
    /// </summary>
    /// <returns></returns>
    private IEnumerator AbortAllWebRequests()
    {
        abortProcessRunning = true;
        //abort 4 times to get over the retry counter
        for (int i = 0; i < 4; i++)
        {
            print("Abort Number: " + i);
            //get list of active web requests
            Assembly libAssembly = Assembly.GetAssembly(typeof(AssetBundleProvider));
            System.Type type = libAssembly.GetType("UnityEngine.ResourceManagement.WebRequestQueue");

            FieldInfo fieldActive = type.GetField("s_ActiveRequests", BindingFlags.Static | BindingFlags.NonPublic);
            List<UnityWebRequestAsyncOperation> requestsActive = (List<UnityWebRequestAsyncOperation>)fieldActive?.GetValue(type);

            //abort every active web request
            foreach (UnityWebRequestAsyncOperation request in requestsActive)
            {
                request.webRequest.Abort();
            }

            StopCoroutine(DownloadLanguagePackage(currentLanguage));
            yield return new WaitForSeconds(0.01f);
        }
        abortProcessRunning = false;
        progressPercentageText.text = "0%";
        progressBar.fillAmount = 0f;
        AssetLabelReference language = new AssetLabelReference();
        language.labelString = currentLanguage;
        Addressables.ClearDependencyCacheAsync(language);
    }

    public bool IsDownloadRunning()
    {
        return downloadRunning;
    }

}
