using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;



public class VignetteRecognitionAndroid : MonoBehaviour
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
    private int maxMovingImages;
    [SerializeField]
    private XRReferenceImageLibrary referenceImageLibrary;


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
    private string currentPage;

    private ARTrackedImageManager arTrackedImageManager;
    private ARRaycastManager arRaycastManager;
    private VoiceLinePlayer voiceLinePlayer;

    private Vector2 screenCenter;

    List<ARRaycastHit> rcHits = new List<ARRaycastHit>();

    List<ARTrackedImage> currentTrackedImageList = new List<ARTrackedImage>();

    private List<GameObject> arTrackedImageObjectList = new List<GameObject>();

    //image display
    private const string testingImagesPath = "Assets/_TrackingVignettes/TestingImages/";
    private const string productionImagesPath = "Assets/_TrackingVignettes/ProductionImages/";
    AsyncOperationHandle<Sprite> textureHandle;
    private bool loadingData = false;
    private Sprite currentLoadedSprite;




    #region Initiation

    private void Awake()
    {
        scanTimer = 0;
        trackingLostTimer = trackingLostTime;
        trackingProgressBar.fillAmount = 0;
        screenCenter = new Vector2(Screen.currentResolution.width / 2, Screen.currentResolution.height / 2);
        print(screenCenter);
        arTrackedImageManager = GameObject.FindObjectOfType<ARTrackedImageManager>();
        arRaycastManager = GameObject.FindObjectOfType<ARRaycastManager>();
        voiceLinePlayer = GameObject.FindObjectOfType<VoiceLinePlayer>();
        viewFinderAnimation = GameObject.FindObjectOfType<ViewfinderAnimation>();

        print(arTrackedImageManager.referenceLibrary[0].name);
    }


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

    }

    private IEnumerator ReInstantiateImageManager()
    {
        //StartCoroutine(DeactivateImageManager());
        arTrackedImageManager.trackedImagesChanged -= OnTrackedImageChanged;

        Destroy(arTrackedImageManager);
        yield return new WaitForEndOfFrame();
        arTrackedImageManager = arRaycastManager.gameObject.AddComponent(typeof(ARTrackedImageManager)) as ARTrackedImageManager;
        arTrackedImageManager.enabled = false;
        arTrackedImageManager.referenceLibrary = referenceImageLibrary;
        arTrackedImageManager.enabled = true;
        arTrackedImageManager.maxNumberOfMovingImages = maxMovingImages;
        arTrackedImageManager.trackedImagesChanged += OnTrackedImageChanged;

    }


    /// <summary>
    /// Add or remove currently tracked images from list
    /// </summary>
    /// <param name="args"></param>
    private void OnTrackedImageChanged(ARTrackedImagesChangedEventArgs args)
    {

        if (scanCompleted)
        {          
            currentTrackedImageList.Clear();
            StartCoroutine(ReInstantiateImageManager());
            scanCompleted = false;
        }

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
            if (image.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                //foreach (ARRaycastHit hit in rcHits)
                //{
                //    if (hit.trackableId == image.trackableId)
                //    {
                //        tracking = true;
                //        currentImage = image;
                //    }
                //}


                if (!currentTrackedImageList.Contains(image))
                {
                    currentTrackedImageList.Add(image);
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
                currentTrackedImageList.Remove(image);


            }

            //updatedTrackablesDebug.text += " " + image.referenceImage.name;

        }


        foreach (ARTrackedImage image in args.added)
        {
            // add image to tracked images list
            currentTrackedImageList.Add(image);

            if(image.referenceImage.name.Split('_')[1] != currentPage)
            {
                StartCoroutine(FauxPageSwap());
                currentPage = image.referenceImage.name.Split('_')[1];
            }

            //Debug.Log(image.referenceImage.name + " ADDED");
            // addedTrackablesDebug.text += " " + image.referenceImage.name;



        }

        foreach (ARTrackedImage image in args.removed)
        {
            currentTrackedImageList.Remove(image);

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

        if (arRaycastManager.Raycast(screenCenter, rcHits, TrackableType.Image))
        {

            foreach (ARRaycastHit hit in rcHits)
            {
                if (raycastIdText != null)
                {
                    raycastIdText.text = hit.trackableId.ToString();
                }
                foreach (ARTrackedImage image in currentTrackedImageList)
                {
                    if (image.trackableId == hit.trackableId)
                    {
                        if (trackableNameText != null)
                        {
                            trackableNameText.text = image.referenceImage.name;
                        }
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
        foreach (ARRaycastHit hit in rcHits)
        {
            foreach (ARTrackedImage image in currentTrackedImageList)
            {
                if (image.trackableId == hit.trackableId)
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
                            print(currentLoadedSprite.name + " sprite sucessfuully loaded");
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


    private IEnumerator FauxPageSwap()
    {
        arTrackedImageManager.subsystem.Stop();
        yield return new WaitForSeconds(0.5f);
        arTrackedImageManager.subsystem.Start();
        pageRecognisedAnimation.PlayRecognisedAnimation();
        StartCoroutine(ReInstantiateImageManager());
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


