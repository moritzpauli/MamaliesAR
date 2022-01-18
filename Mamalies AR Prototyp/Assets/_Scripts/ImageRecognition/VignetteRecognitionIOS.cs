using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using UnityEngine.Jobs;
#if UNITY_IOS
using UnityEngine.iOS;
#endif

public class VignetteRecognitionIOS : MonoBehaviour
{
    //debug
    [SerializeField]
    private TextMeshProUGUI raycastIdText;
    [SerializeField]
    private TextMeshProUGUI trackableNameText;
    //[SerializeField]
    //private TextMeshProUGUI addedTrackablesDebug;
    //[SerializeField]
    //private TextMeshProUGUI updatedTrackablesDebug;
    //[SerializeField]
    //private TextMeshProUGUI removedTrackablesDebug;
    [SerializeField]
    private Text currentLibraryEntry;

    // parameters
    [SerializeField]
    private float scanTime;
    private float scanTimer;
    [SerializeField]
    private bool animateViewfinder = false;
    [SerializeField]
    private LayerMask arMarkerLayer;



    // effects
    [SerializeField]
    private Image trackingProgressBar;
    private ViewfinderAnimation viewFinderAnimation;
    [SerializeField]
    private Image scannedImageDisplayHorizontal;
    [SerializeField]
    private Image scannedImageDisplayVertical;
    [SerializeField]
    private GameObject scannedImageBackground;
    [SerializeField]
    private RecognisedAnimation pageRecognisedAnimation;


    // func
    [SerializeField]
    private Camera arCamera;
    //Ar Tracking Prefabs
    [SerializeField]
    private GameObject trackingMarker;

    //tracking process
    private bool resetTracking = false;
    bool tracking;
    bool imageSwap = true;
    private string currentImage;
    private string previousImage;
    private float trackingLostTime = 0.2f;
    private float imageChangedTime = 0.4f;
    private float trackingLostTimer;
    private float imageChangedTimer;

    [SerializeField]
    private ARTrackedImageManager arTrackedImageManager;
    private VoiceLinePlayer voiceLinePlayer;

    private Vector2 screenCenter;


    private List<GameObject> arTrackedImageObjectList = new List<GameObject>();

    private GameObject trackingManagerGameobject;

    //image display
    private const string testingImagesPath = "Assets/_TrackingVignettes/TestingImages/";
    private const string productionImagesPath = "Assets/_TrackingVignettes/ProductionImages/";
    AsyncOperationHandle<Sprite> textureHandle;
    private bool loadingData = false;
    private Sprite currentLoadedSprite;

    //IOS tracking
    private RaycastHit physicsRaycastHit;
    private string[] imageLibraryPaths;
    [SerializeField]
    private List<XRReferenceImageLibrary> imageLibrariesList = new List<XRReferenceImageLibrary>();
    private const string libraryPath = "Assets/_TrackingVignettes/ImageLibrary/VignetteLibrariesIOS/";
    AsyncOperationHandle<XRReferenceImageLibrary> libraryHandle;

    [SerializeField]
    private List<RuntimeReferenceImageLibrary> runtimeImageLibrariesList = new List<RuntimeReferenceImageLibrary>();

    [SerializeField]
    private XRReferenceImageLibrary singlePagesLibrary;
    [SerializeField]
    private XRReferenceImageLibrary doublePagesLibrary;

    [SerializeField]
    private bool useDoublePages = false;


    private bool imagesChanged = false;

    private bool pageSelection = false;

    private MutableRuntimeReferenceImageLibrary mutableSinglePageLibrary;
    private MutableRuntimeReferenceImageLibrary mutableDoublePageLibrary;

    private MutableRuntimeReferenceImageLibrary mutableRuntimeLibrary;


    [SerializeField]
    private bool startupConvertLibraries = false;

    [SerializeField]
    private Texture2D[] pageReferenceImages;

    private RuntimeReferenceImageLibrary tempLibrary;

    [SerializeField]
    private string currentPage = "";

    [SerializeField]
    private bool useMutableLibrary;
    AsyncOperationHandle<IList<Texture2D>> trackingTextureHandle;
    AsyncOperationHandle<IList<Texture2D>> pageReferenceTextureHandle;

  

    //ios generation
#if UNITY_IOS
    [SerializeField]
    private DeviceGeneration[] inferiorDevices;
#endif
    private bool olderDevice = false;


    //UI
    [SerializeField]
    private GameObject loadingPanel;



    [SerializeField]
    private float loadNewLibraryTime = 1.3f;
    [SerializeField]
    private float loadNewLibraryTimeInitalDelay = 0.7f;

    private float loadNewLibraryTimer;
    private float pageSelectionClearTimer;

    [SerializeField]
    private Texture2D testTex;



    #region Initiation

    private void Awake()
    {
        scanTimer = 0;
        loadNewLibraryTimer = loadNewLibraryTime;
        pageSelectionClearTimer = loadNewLibraryTime;
        trackingLostTimer = trackingLostTime;
        trackingProgressBar.fillAmount = 0;
        screenCenter = new Vector2(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2);
        //arTrackedImageManager = GameObject.FindObjectOfType<ARTrackedImageManager>();
        voiceLinePlayer = GameObject.FindObjectOfType<VoiceLinePlayer>();
        viewFinderAnimation = GameObject.FindObjectOfType<ViewfinderAnimation>();
        trackingManagerGameobject = arTrackedImageManager.gameObject;
        //loadingPanel.SetActive(true);
        //currentPage = imageLibrariesList[0].name;
        //ConvertLibrariesAsync();

        //foreach (Texture2D tex in pageReferenceImages)
        //	{
        //		mutableRuntimeLibrary.ScheduleAddImageWithValidationJob(tex, tex.name, 0.2f);
        //	}


       
        
            mutableSinglePageLibrary = (MutableRuntimeReferenceImageLibrary)arTrackedImageManager.CreateRuntimeLibrary(singlePagesLibrary);
        
            mutableDoublePageLibrary = (MutableRuntimeReferenceImageLibrary)arTrackedImageManager.CreateRuntimeLibrary(doublePagesLibrary);
        

        for(int i = 0; i < mutableSinglePageLibrary.supportedTextureFormatCount; i++)
        {
            print(mutableSinglePageLibrary.GetSupportedTextureFormatAt(i));
        }
        print(arTrackedImageManager.referenceLibrary[0].name);

#if UNITY_IOS
        foreach(DeviceGeneration generation in inferiorDevices)
        {
            if(Device.generation == generation)
            {
                olderDevice = true;
            }
        }
#endif

        print(testTex.format + "TEXFORM");
    }

    //#if UNITY_EDITOR
    //	/// <summary>
    //	/// execute in editor, load image libraries
    //	/// </summary>

    //	private void OnValidate()
    //	{

    //		imageLibrariesList.Clear();


    //		imageLibraryPaths = Directory.GetFiles(libraryPath, "*.asset");

    //		foreach (string libraryFilePath in imageLibraryPaths)
    //		{
    //			//Debug.Log(imageFilePath);
    //			imageLibrariesList.Add((XRReferenceImageLibrary)AssetDatabase.LoadAssetAtPath(libraryFilePath, typeof(XRReferenceImageLibrary)));
    //			// runtimeImageLibrariesList.Add((RuntimeReferenceImageLibrary)AssetDatabase.LoadAssetAtPath(libraryFilePath, typeof(RuntimeReferenceImageLibrary)));
    //		}



    //#if UNITY_EDITOR_OSX
    //arTrackedImageManager = GameObject.FindObjectOfType<ARTrackedImageManager>();
    //    arTrackedImageManager.referenceLibrary = imageLibrariesList[0];
    //#endif
    //	}
    //#endif

    private void OnEnable()
    {
        arTrackedImageManager.trackedImagesChanged += OnTrackedImageChanged;
    }

    private void OnDisable()
    {
        arTrackedImageManager.trackedImagesChanged -= OnTrackedImageChanged;
    }

    #endregion

    private void Update()
    {
        HideScannedImage();
        ArRaycast();

        CompareRaycastTrack();

        TrackedImageScanningProcess();

        SwapImageLibraries();

   
    }



    //private IEnumerator ResetTracking()
    //{
    //    tempLibrary = arTrackedImageManager.subsystem.imageLibrary;
    //    arTrackedImageManager.trackedImagesChanged -= OnTrackedImageChanged;
    //    Destroy(arTrackedImageManager);
    //    yield return new WaitForEndOfFrame();
    //    arTrackedImageManager = trackingManagerGameobject.AddComponent(typeof(ARTrackedImageManager)) as ARTrackedImageManager;
    //    arTrackedImageManager.enabled = false;
    //    arTrackedImageManager.subsystem.imageLibrary = tempLibrary;
    //    arTrackedImageManager.enabled = true;
    //    arTrackedImageManager.maxNumberOfMovingImages = 10;
    //    arTrackedImageManager.trackedImagesChanged += OnTrackedImageChanged;
    //    resetTracking = true;
    //}

    public void SwapImageLibraries()
    {
        //StartCoroutine(AppendPageReferenceLibrary());
        if (!imagesChanged && !pageSelection)
        {
            loadNewLibraryTimer -= Time.deltaTime;

            if (loadNewLibraryTimer <= 0)
            {

                StartCoroutine(AppendPageReferenceLibrary());
            }
        }
        else
        {
            loadNewLibraryTimer = loadNewLibraryTime;
        }

        imagesChanged = false;
        if (currentLibraryEntry != null)
        {
            currentLibraryEntry.text = loadNewLibraryTimer.ToString();
        }
    }

    private IEnumerator AppendPageReferenceLibrary()
    {
        loadNewLibraryTimer = loadNewLibraryTime;
        pageSelectionClearTimer = loadNewLibraryTime;
        pageSelection = true;
        print("Append Started");
        if (trackingTextureHandle.IsValid())
        {
            Addressables.Release(trackingTextureHandle);

        }

        if (!doublePagesLibrary)
        {
            mutableRuntimeLibrary = mutableSinglePageLibrary;
        }
        else
        {
            mutableRuntimeLibrary = mutableDoublePageLibrary;
        }
        //arTrackedImageManager.enabled = false;
        //arTrackedImageManager.enabled = true;
        if (currentPage != null && currentPage != "")
        {
            trackingTextureHandle = Addressables.LoadAssetsAsync<Texture2D>(currentPage, null);
            yield return new WaitUntil(() => trackingTextureHandle.IsDone);
            loadNewLibraryTimer = loadNewLibraryTime;
            print("LOADED - " + currentPage);
            foreach (Texture2D tex in trackingTextureHandle.Result)
            {
                AddReferenceImageJobState addJobState = mutableRuntimeLibrary.ScheduleAddImageWithValidationJob(tex, tex.name, (float)tex.width / 7f * 0.001f);
                addJobState.jobHandle.Complete();
                loadNewLibraryTimer = loadNewLibraryTime;
            }
            print("ADDED TO LIBRARY - " + currentPage);
        }

        arTrackedImageManager.subsystem.imageLibrary = mutableRuntimeLibrary;
        arTrackedImageManager.subsystem.Start();
        loadNewLibraryTimer = loadNewLibraryTime;
        //StartCoroutine(ResetTracking());
        //resetTracking = true;
        DestroyTrackingObjects();
        yield return null;
    }


    /// <summary>
    /// Add or remove currently tracked images from list
    /// </summary>
    /// <param name="args"></param>
    private void OnTrackedImageChanged(ARTrackedImagesChangedEventArgs args)
    {
        //print("any tracking change");
        if (resetTracking)
        {

            args.removed.Clear();
            args.added.Clear();
            args.updated.Clear();
            resetTracking = false;
            print("CLEARED ALL TRACKING LISTS");

        }
        imagesChanged = true;

        foreach (ARTrackedImage image in args.added)
        {
            //print(image.referenceImage.name + "ADDED");
            // add image to tracked images list
            if (char.IsDigit(image.referenceImage.name[0]))
            {
                AddTrackedObject(image);
            }


            if (!char.IsDigit(image.referenceImage.name[0]) && pageSelection && currentPage != image.referenceImage.name)
            {
                SelectNewPageLibrary(image.referenceImage.name);
                print("PGREC: " + image.referenceImage.name);
            }

            //Debug.Log(image.referenceImage.name + " ADDED");
            // addedTrackablesDebug.text += " " + image.referenceImage.name;

            

        }


        foreach (ARTrackedImage image in args.updated)
        {
            if (!char.IsDigit(image.referenceImage.name[0]) && pageSelection && currentPage != image.referenceImage.name)
            {
                print("PGREC: " + image.referenceImage.name);
                SelectNewPageLibrary(image.referenceImage.name);
            }

            if (image.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.None)
            {
                RemoveTrackedObject(image);
            }

            if (image.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                UpdateTrackedObject(image);
                if (char.IsDigit(image.referenceImage.name[0]))
                {
                    bool objectAdded = false;
                    foreach (GameObject gObject in arTrackedImageObjectList)
                    {
                        if(gObject.name == image.referenceImage.name)
                        {
                            objectAdded = true;
                        }
                    }

                    if (!objectAdded)
                    {
                        AddTrackedObject(image);
                    }
                }
            }
        }

        foreach (ARTrackedImage image in args.removed)
        {
            RemoveTrackedObject(image);
        }


    }

    private void DestroyTrackingObjects()
    {
        foreach (GameObject go in arTrackedImageObjectList)
        {
            Destroy(go);
        }
        GameObject[] tempMarkers = GameObject.FindGameObjectsWithTag("arMarker");
        foreach (GameObject go in tempMarkers)
        {
            Destroy(go);
        }
        arTrackedImageObjectList.Clear();
    }


    /// <summary>
    /// Sets a new page library and resets the tracking
    /// </summary>
    /// <param name="pageName"></param>
    private void SelectNewPageLibrary(string pageName)
    {

        pageRecognisedAnimation.PlayRecognisedAnimation();
        if (pageName.Contains("x"))
        {
            pageName = pageName.Trim('x');
        }
        print("Select Page: " + pageName);

        arTrackedImageManager.enabled = false;
        arTrackedImageManager.referenceLibrary = null;
        if (!useMutableLibrary)
        {
            foreach (XRReferenceImageLibrary lib in imageLibrariesList)
            {
                if (lib.name == pageName)
                {
                    arTrackedImageManager.referenceLibrary = lib;
                    break;
                }
            }

            currentPage = pageName;
            loadNewLibraryTimer = loadNewLibraryTime + loadNewLibraryTimeInitalDelay;
            arTrackedImageManager.enabled = true;
            //StartCoroutine(ResetTracking());
            resetTracking = true;
            DestroyTrackingObjects();
            print("Library Asset Loaded: " + pageName);
            pageSelection = false;
        }
        //if (useMutableLibrary)
        //{
        //    currentPage = pageName;
        //    mutableRuntimeLibrary = mutablePageReferenceLibrary;
        //    arTrackedImageManager.subsystem.imageLibrary = mutableRuntimeLibrary;

        //    if (trackingTextureHandle.IsValid())
        //    {
        //        Addressables.Release(trackingTextureHandle);

        //    }
        //    //if (pageReferenceTextureHandle.IsValid())
        //    //{
        //    //    Addressables.Release(pageReferenceTextureHandle);
        //    //}

        //    //foreach (Texture2D tex in pageReferenceImages)
        //    //{
        //    //	mutableRuntimeLibrary.ScheduleAddImageWithValidationJob(tex, tex.name, 0.2f);
        //    //}


        //    //load in textures of current page
        //    trackingTextureHandle = Addressables.LoadAssetsAsync<Texture2D>(pageName, null);
        //    yield return new WaitUntil(() => trackingTextureHandle.IsDone);
        //    print("LOADED - " + pageName);
        //    foreach (Texture2D tex in trackingTextureHandle.Result)
        //    {
        //        AddReferenceImageJobState addJobState = mutableRuntimeLibrary.ScheduleAddImageWithValidationJob(tex, tex.name, (float)tex.width / 7f * 0.001f);
        //        //yield return new WaitUntil(() => addJobState.jobHandle.IsCompleted);
        //    }

        //    print("ADDED TO LIBRARY - " + pageName);

        //    ////load in textures of page references
        //    //pageReferenceTextureHandle = Addressables.LoadAssetsAsync<Texture2D>("pageReference", null);
        //    //yield return new WaitUntil(() => pageReferenceTextureHandle.IsDone);
        //    //print("LOADED - reference images");
        //    //foreach (Texture2D tex in pageReferenceTextureHandle.Result)
        //    //{
        //    //    Unity.Jobs.JobHandle addJobHandle =  mutableRuntimeLibrary.ScheduleAddImageJob(tex, tex.name, 0.2f);
        //    //    yield return new WaitUntil(() => addJobHandle.IsCompleted);
        //    //    addJobHandle.Complete();


        //    //}

        //    //print("ADDED TO LIBRARY - reference images");


        //}


    }


    private void AddTrackedObject(ARTrackedImage image)
    {

        if (char.IsDigit(image.referenceImage.name[0]))
        {
            print("Added tracked object: " + image.referenceImage.name);
            GameObject trackedObject = Instantiate(trackingMarker);
            trackedObject.transform.name = image.referenceImage.name;
            trackedObject.transform.position = image.transform.position;
            trackedObject.transform.rotation = image.transform.rotation;
            trackedObject.transform.localScale = new Vector3(image.referenceImage.size.x, 0.005f, image.referenceImage.size.y);
            arTrackedImageObjectList.Add(trackedObject);
        }
        else
        {
            return;
        }
    }

    private void UpdateTrackedObject(ARTrackedImage image)
    {
        bool imageExists = false;
        foreach (GameObject gObject in arTrackedImageObjectList)
        {
            if (gObject.name == image.referenceImage.name)
            {
                gObject.transform.position = image.transform.position;
                gObject.transform.rotation = image.transform.rotation;
                //print("Update Tracking: " + image.referenceImage.name);
                imageExists = true;
            }
        }
        if (!imageExists)
        {
            AddTrackedObject(image);
        }
    }

    private void RemoveTrackedObject(ARTrackedImage image)
    {
        for (int i = 0; i < arTrackedImageObjectList.Count; i++)
        {
            if (arTrackedImageObjectList[i].name == image.referenceImage.name)
            {
                Destroy(arTrackedImageObjectList[i]);
                arTrackedImageObjectList.RemoveAt(i);
            }
        }
    }


    //private void PhysicsRaycastTest()
    //{
    //    RaycastHit testHit;
    //    Ray testRay = arCamera.ScreenPointToRay(new Vector3(screenCenter.x, screenCenter.y, 0f));
    //    if (Physics.Raycast(testRay, out testHit))
    //    {
    //        print(testHit.transform.name);
    //    }
    //}

    /// <summary>
    /// Cast an AR ray 
    /// </summary>
    private void ArRaycast()
    {

        Ray centerRay = arCamera.ScreenPointToRay(new Vector3(screenCenter.x, screenCenter.y, 0f));
        if (Physics.Raycast(centerRay, out physicsRaycastHit, arMarkerLayer))
        {
            if (raycastIdText != null)
            {
                raycastIdText.text = physicsRaycastHit.transform.gameObject.GetInstanceID().ToString();
            }
            foreach (GameObject gObject in arTrackedImageObjectList)
            {
                if (gObject.name == physicsRaycastHit.transform.name)
                {
                    if (trackableNameText != null)
                    {
                        trackableNameText.text = gObject.name;
                    }
                }
            }
        }
        else
        {
            if (raycastIdText != null)
            {
                raycastIdText.text = "NONE";
            }
        }
    }

    /// <summary>
    /// Checks one of the current raycast results is the same as one of the currently tracked images
    /// </summary>
    private void CompareRaycastTrack()
    {

        tracking = false;

        if (physicsRaycastHit.transform != null)
        {
            foreach (GameObject gObject in arTrackedImageObjectList)
            {
                if (gObject.name == physicsRaycastHit.transform.name)
                {
                    tracking = true;
                    currentImage = gObject.name;
                }

            }
        }


    }

    /// <summary>
    /// Call the Play voiceline Funtion (play according voiceline in both languages)
    /// </summary>
    /// <param name="vignetteName"></param>
    private void ScanCompleted(string vignetteName)
    {
        if (vignetteName[0] == '#')
        {
            print("SCANNED: " + vignetteName);
            return;
        }
        scanTimer = 0;
        voiceLinePlayer.PlayVoiceLine(vignetteName);
        //StartCoroutine(ResetTracking());
        //DestroyTrackingObjects();
    }

    /// <summary>
    /// Times the scanning and CALLS AnimateViewfinder during the scanning process, when done CALLS ScanCompleted
    /// and DisplayScannedImage
    /// </summary>
    private void TrackedImageScanningProcess()
    {
        if (tracking)
        {
            if (!voiceLinePlayer.HasLinePlaying())
            {
                if (imageSwap)
                {
                    //start loading image in
                    if (textureHandle.IsValid())
                    {
                        Addressables.Release(textureHandle.Result);
                    }


                    if (currentImage[0] == '#')
                    {
                        loadingData = true;
                        string filePath = testingImagesPath + currentImage.Substring(1) + ".png";
                        textureHandle = Addressables.LoadAssetAsync<Sprite>(filePath);
                        textureHandle.Completed += (operation) =>
                        {
                            this.currentLoadedSprite = textureHandle.Result;
                            loadingData = false;
                            //Addressables.Release(textureHandle);
                        };
                    }
                    else
                    {
                        loadingData = true;
                        string filePath = productionImagesPath + currentImage + ".png";
                        textureHandle = Addressables.LoadAssetAsync<Sprite>(filePath);
                        textureHandle.Completed += (operation) =>
                        {
                            this.currentLoadedSprite = textureHandle.Result;
                            loadingData = false;
                            //Addressables.Release(textureHandle);
                            //print(currentLoadedSprite.name + " sprite sucessfuully loaded");
                        };
                    }

                    previousImage = currentImage;
                    imageSwap = false;
                }
                trackingLostTimer = trackingLostTime;

                scanTimer += Time.deltaTime;
                trackingProgressBar.fillAmount = scanTimer / scanTime;
                //viewFinderAnimation.AnimateViewfinder(scanTimer);
                if (animateViewfinder)
                {
                    viewFinderAnimation.StartViewfinderAnimation();
                }
                if (scanTimer > scanTime)
                {

                    ScanCompleted(currentImage);
                    StartCoroutine(DisplayScannedImage());
                    //#if UNITY_ANDROID || UNITY_IOS

                    //                    Handheld.Vibrate();

                    //#endif

                    //print("RECOGNISED: " + currentImage.referenceImage.name);
                    if (animateViewfinder)
                    {
                        viewFinderAnimation.StopViewfinderAnimation();

                    }

                }
            }
            if (currentImage != previousImage)
            {
                if (animateViewfinder)
                {
                    viewFinderAnimation.StopViewfinderAnimation();
                }
                imageChangedTimer -= Time.deltaTime;
                if (imageChangedTimer < 0)
                {
                    currentImage = null;
                    imageSwap = true;
                    scanTimer = 0;
                    trackingLostTimer = trackingLostTime;
                    imageChangedTimer = imageChangedTime;
                    trackingProgressBar.fillAmount = 0;
                }
            }
            else
            {
                imageChangedTimer = imageChangedTime;
            }
        }
        else
        {
            if (animateViewfinder)
            {
                viewFinderAnimation.StopViewfinderAnimation();
            }
            trackingLostTimer -= Time.deltaTime;
            if (trackingLostTimer < 0)
            {
                //print("clear images");
                currentImage = null;
                scanTimer = 0;
                trackingLostTimer = trackingLostTime;
                imageChangedTimer = imageChangedTime;
                trackingProgressBar.fillAmount = 0;
            }
        }
    }

    #region PostScan

    /// <summary>
    /// Converts the reference image to a sprite, adjusts the size of the Image(Gameobject) rects to fit the reference 
    /// images aspect ratio and displays it
    /// </summary>
    /// <param name="imageName"></param>
    private IEnumerator DisplayScannedImage()
    {
        //print(image.referenceImage.name);
        // viewFinderAnimation.DeActivateViewfinder();
        scannedImageDisplayHorizontal.enabled = false;
        scannedImageDisplayVertical.enabled = false;
        scannedImageBackground.SetActive(true);
        //Texture2D imageTex = image.referenceImage.texture;

        yield return new WaitUntil(() => loadingData == false);
        float heightRatio = (float)currentLoadedSprite.texture.height / (float)currentLoadedSprite.texture.width;
        //Rect texRect = new Rect(0, 0, currentLoadedTexture.width, currentLoadedTexture.height);
        if (heightRatio < 2f)
        {
            scannedImageDisplayHorizontal.enabled = true;
            Sprite imageSprite = currentLoadedSprite;
            scannedImageDisplayHorizontal.sprite = imageSprite;
            scannedImageDisplayHorizontal.rectTransform.sizeDelta = new Vector2(scannedImageDisplayHorizontal.rectTransform.sizeDelta.x, scannedImageDisplayHorizontal.rectTransform.sizeDelta.x * heightRatio);
        }
        else
        {
            scannedImageDisplayVertical.enabled = true;
            Sprite imageSprite = currentLoadedSprite;
            scannedImageDisplayVertical.sprite = imageSprite;
            scannedImageDisplayVertical.rectTransform.sizeDelta = new Vector2(scannedImageDisplayVertical.rectTransform.sizeDelta.y / heightRatio, scannedImageDisplayVertical.rectTransform.sizeDelta.y);
        }

    }






    /// <summary>
    /// Hides the display of the scanned image
    /// </summary>
    private void HideScannedImage()
    {
        if ((scannedImageDisplayHorizontal.enabled || scannedImageDisplayVertical.enabled) && !voiceLinePlayer.HasLinePlaying())
        {
            scannedImageDisplayHorizontal.enabled = false;
            scannedImageDisplayVertical.enabled = false;
            scannedImageBackground.SetActive(false);

        }

    }




    #endregion


    public void ChangePageReferenceLibrary(bool usedouble)
    {
        useDoublePages = usedouble;
    }


}
