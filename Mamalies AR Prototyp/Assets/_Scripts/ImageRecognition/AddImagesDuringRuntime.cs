using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class AddImagesDuringRuntime : MonoBehaviour
{


    [Tooltip("The scenes tracked image manager")]
    [SerializeField]
    private ARTrackedImageManager trackedImageManager;


    [SerializeField]
    private Texture2D[] imagesToAdd;

    public MutableRuntimeReferenceImageLibrary runtimeImageLibrary;

    // Start is called before the first frame update
    void Start()
    {
        runtimeImageLibrary = (MutableRuntimeReferenceImageLibrary)trackedImageManager.CreateRuntimeLibrary();
        trackedImageManager.enabled = false;
        trackedImageManager.referenceLibrary = runtimeImageLibrary;
        trackedImageManager.enabled = true;

        RuntimeAddImages(imagesToAdd);
        print(trackedImageManager.referenceLibrary.count);
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void RuntimeAddImages(Texture2D[] images)
    {
        if (runtimeImageLibrary is MutableRuntimeReferenceImageLibrary mutableLibrary)
        {
            for (int i = 0; i < images.Length; i++)
            {
                //mutableLibrary.ScheduleAddImageJob(images[i], images[i].name, 0.1f));
                //mutableLibrary.ScheduleAddImageWithValidationJob(images[i], images[i].name, new Vector2(0.1f, 0.1f));
                print(mutableLibrary.ScheduleAddImageWithValidationJob(images[i], images[i].name, 0.1f));


            }

           
        }

    }
}
