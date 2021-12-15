using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;


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


    // func
    [SerializeField]
    private Camera arCamera;
    //Ar Tracking Prefabs
    [SerializeField]
    private GameObject trackingMarker;

    //tracking process
    private bool scanCompleted = false;
    bool tracking;
    bool imageSwap = true;
    private ARTrackedImage currentImage;
    private ARTrackedImage previousImage;
    private float trackingLostTime = 0.2f;
    private float imageChangedTime = 0.4f;
    private float trackingLostTimer;
    private float imageChangedTimer;

    private ARTrackedImageManager arTrackedImageManager;
    private VoiceLinePlayer voiceLinePlayer;

    private Vector2 screenCenter;

    List<ARRaycastHit> rcHits = new List<ARRaycastHit>();

    List<ARTrackedImage> currentTrackedImageList = new List<ARTrackedImage>();

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

    [SerializeField]
    private List<RuntimeReferenceImageLibrary> runtimeImageLibrariesList = new List<RuntimeReferenceImageLibrary>();

    [SerializeField]
    private XRReferenceImageLibrary pagesLibrary;

    private RuntimeReferenceImageLibrary runtimePagesLibrary;

    private int imgLibraryIndex = 0;

    private bool imagesChanged = false;

    private bool pageSelection = false;
    private bool convertRuntimeLibraries = true;

    //UI
    [SerializeField]
    private GameObject loadingPanel;



    [SerializeField]
    private float loadNewLibraryTime = 0.6f;

    private float loadNewLibraryTimer = 0.6f;

    #region Initiation

    private void Awake()
    {       
        scanTimer = 0;
        trackingLostTimer = trackingLostTime;
        trackingProgressBar.fillAmount = 0;
        screenCenter = new Vector2(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2);
        print(screenCenter);
        arTrackedImageManager = GameObject.FindObjectOfType<ARTrackedImageManager>();
        voiceLinePlayer = GameObject.FindObjectOfType<VoiceLinePlayer>();
        viewFinderAnimation = GameObject.FindObjectOfType<ViewfinderAnimation>();
        trackingManagerGameobject = arTrackedImageManager.gameObject;
        
    }

    #if UNITY_EDITOR
    /// <summary>
    /// execute in editor, load image libraries
    /// </summary>

    private void OnValidate()
    {

        imageLibrariesList.Clear();
     

        imageLibraryPaths = Directory.GetFiles(libraryPath, "*.asset");

        foreach (string libraryFilePath in imageLibraryPaths)
        {
            //Debug.Log(imageFilePath);
            imageLibrariesList.Add((XRReferenceImageLibrary)AssetDatabase.LoadAssetAtPath(libraryFilePath, typeof(XRReferenceImageLibrary)));
            // runtimeImageLibrariesList.Add((RuntimeReferenceImageLibrary)AssetDatabase.LoadAssetAtPath(libraryFilePath, typeof(RuntimeReferenceImageLibrary)));
        }



#if UNITY_EDITOR_OSX
arTrackedImageManager = GameObject.FindObjectOfType<ARTrackedImageManager>();
    arTrackedImageManager.referenceLibrary = imageLibrariesList[0];
#endif
    }
#endif

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
        if (convertRuntimeLibraries)
        {
            ConvertLibrariesToRuntime();
        }
        HideScannedImage();
        ArRaycast();

        CompareRaycastTrack();

        TrackedImageScanningProcess();

        SwapImageLibraries();
        
    }

    public void CycleImageLibrariesManual()
    {
        imgLibraryIndex++;
        if (imgLibraryIndex > imageLibrariesList.Count - 1)
        {
            imgLibraryIndex = 0;
        }
        print(imageLibrariesList[imgLibraryIndex].name);
        arTrackedImageManager.enabled = false;
        arTrackedImageManager.subsystem.imageLibrary = runtimeImageLibrariesList[imgLibraryIndex];
        arTrackedImageManager.enabled = true;
    }

    private void ConvertLibrariesToRuntime()
    {
        //StartCoroutine(ConvertLibrariesCoroutine());
        ConvertLibrariesAsync();
        convertRuntimeLibraries = false;

    }

    private async Task ConvertLibrariesAsync()
    {
        loadingPanel.SetActive(true);
        for (int i = 0; i < imageLibrariesList.Count; i++)
        {
            runtimeImageLibrariesList.Add(arTrackedImageManager.CreateRuntimeLibrary(imageLibrariesList[i]));
        }
        runtimePagesLibrary = arTrackedImageManager.CreateRuntimeLibrary(pagesLibrary);
        loadingPanel.SetActive(false);
        await Task.Delay(0);
    }

    private IEnumerator ConvertLibrariesCoroutine()
    {
        loadingPanel.SetActive(true);
        for (int i = 0; i < imageLibrariesList.Count; i++)
        {
            runtimeImageLibrariesList.Add(arTrackedImageManager.CreateRuntimeLibrary(imageLibrariesList[i]));
        }
        runtimePagesLibrary = arTrackedImageManager.CreateRuntimeLibrary(pagesLibrary);
        loadingPanel.SetActive(false);
        yield return null;
    }

    private IEnumerator ResetTracking()
    {
        arTrackedImageManager.trackedImagesChanged -= OnTrackedImageChanged;
        Destroy(arTrackedImageManager);
        yield return new WaitForEndOfFrame();
        arTrackedImageManager = trackingManagerGameobject.AddComponent(typeof(ARTrackedImageManager)) as ARTrackedImageManager;
        arTrackedImageManager.subsystem.imageLibrary = runtimeImageLibrariesList[imgLibraryIndex];
        arTrackedImageManager.maxNumberOfMovingImages = 10;
        arTrackedImageManager.trackedImagesChanged += OnTrackedImageChanged;
        print(7);
        
        

    }

    private void SwapImageLibraries()
    {
        if (!imagesChanged && !pageSelection)
        {
            loadNewLibraryTimer -= Time.deltaTime;
            if (loadNewLibraryTimer <= 0)
            {
                print("swap libraries");
                arTrackedImageManager.subsystem.imageLibrary = runtimePagesLibrary;
                //StartCoroutine(ResetTracking());
                pageSelection = true;               
                loadNewLibraryTimer = loadNewLibraryTime;
            }
        }
        else
        {
            loadNewLibraryTimer = loadNewLibraryTime;
        }

        imagesChanged = false;
    }

    /// <summary>
    /// Add or remove currently tracked images from list
    /// </summary>
    /// <param name="args"></param>
    private void OnTrackedImageChanged(ARTrackedImagesChangedEventArgs args)
    {
        print("any tracking change");
        if (scanCompleted)
        {
            args.removed.Clear();
            args.added.Clear();
            args.updated.Clear();
            scanCompleted = false;
        }
        imagesChanged = true;
        //addedTrackablesDebug.text = "ADDED: ";
        //updatedTrackablesDebug.text = "UPDATED: ";
        //removedTrackablesDebug.text = "REMOVED: ";
        //Debug.Log("On Tracked Image Changed");
        foreach (ARTrackedImage image in args.updated)
        {
            ////UpdateTrackedObject(image);
            //trackingIndicator.transform.position = image.transform.position;
            //trackingIndicator.transform.rotation = image.transform.rotation;
            //trackingIndicator.transform.localScale = new Vector3(image.referenceImage.size.x, 0.01f, image.referenceImage.size.y);
            ////trackingIndicator.transform.localScale = image.transform.localScale;
            //print(trackingIndicator.transform.position);
            if (image.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking || image.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Limited)
            {
                //foreach (ARRaycastHit hit in rcHits)
                //{
                //    if (hit.trackableId == image.trackableId)
                //    {
                //        tracking = true;
                //        currentImage = image;
                //    }
                //}

                if (pageSelection)
                {

                    SelectNewPageLibrary(image.referenceImage.name);
                }

                UpdateTrackedObject(image);


                if (!currentTrackedImageList.Contains(image))
                {
                    currentTrackedImageList.Add(image);
                    AddTrackedObject(image);
                }
                //foreach(ARTrackedImage imgDebug in currentTrackedImageList)
                //{
                //    print(imgDebug.referenceImage.name);
                //}
                //trackingIdText.text = image.trackableId.ToString();
                //Debug.Log(image.referenceImage.name + " TRACKING");

                // arTrackedImagePrefab.transform.localScale = image.transform.localScale;

            }
            if (image.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.None)
            {
                //print(image.referenceImage.name + " LOST TRACKING");
                //tracking = false;
                //Debug.Log(image.referenceImage.name + " REMOVED");
                //currentTrackedImageList.Remove(image);

                RemoveTrackedObject(image);


            }

            //updatedTrackablesDebug.text += " " + image.referenceImage.name;

        }


        foreach (ARTrackedImage image in args.added)
        {
            // add image to tracked images list
            currentTrackedImageList.Add(image);

            AddTrackedObject(image);

            if (pageSelection)
            {

                SelectNewPageLibrary(image.referenceImage.name);
            }

            //Debug.Log(image.referenceImage.name + " ADDED");
            // addedTrackablesDebug.text += " " + image.referenceImage.name;

            trackingMarker.transform.position = image.transform.position;

        }

        foreach (ARTrackedImage image in args.removed)
        {

            RemoveTrackedObject(image);


        }


    }


    private void SelectNewPageLibrary(string pageName)
    {
        print(pageName);
        for(int i = 0; i < imageLibrariesList.Count; i++)
        {
            if(imageLibrariesList[i].name == pageName)
            {
                imgLibraryIndex = i;
                arTrackedImageManager.subsystem.imageLibrary = runtimeImageLibrariesList[i];
            }
        }
        pageSelection = false;
        loadNewLibraryTimer = loadNewLibraryTime;
        foreach (GameObject go in arTrackedImageObjectList)
        {
            Destroy(go);
        }
        arTrackedImageObjectList.Clear();
        currentTrackedImageList.Clear();
        StartCoroutine( ResetTracking());

    }

    private void AddTrackedObject(ARTrackedImage image)
    {
        GameObject trackedObject = Instantiate(trackingMarker);
        trackedObject.transform.name = image.referenceImage.name;
        trackedObject.transform.position = image.transform.position;
        trackedObject.transform.rotation = image.transform.rotation;
        trackedObject.transform.localScale = new Vector3(image.referenceImage.size.x, 0.005f, image.referenceImage.size.y);
        arTrackedImageObjectList.Add(trackedObject);
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
            foreach (ARTrackedImage image in currentTrackedImageList)
            {
                if (image.referenceImage.name == physicsRaycastHit.transform.name)
                {
                    if (trackableNameText != null)
                    {
                        trackableNameText.text = image.referenceImage.name;
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
            foreach (ARTrackedImage image in currentTrackedImageList)
            {
                if (image.referenceImage.name == physicsRaycastHit.transform.name)
                {
                    tracking = true;
                    currentImage = image;
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
        scanCompleted = true;
        foreach (GameObject go in arTrackedImageObjectList)
        {
            Destroy(go);
        }
        arTrackedImageObjectList.Clear();
        currentTrackedImageList.Clear();
        StartCoroutine(ResetTracking());
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


                    if (currentImage.referenceImage.name[0] == '#')
                    {
                        loadingData = true;
                        string filePath = testingImagesPath + currentImage.referenceImage.name.Substring(1) + ".png";
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
                        string filePath = productionImagesPath + currentImage.referenceImage.name + ".png";
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

                    ScanCompleted(currentImage.referenceImage.name);
                    StartCoroutine(DisplayScannedImage(currentImage));
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
    /// <param name="image"></param>
    private IEnumerator DisplayScannedImage(ARTrackedImage image)
    {
        print(image.referenceImage.name);
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


}
