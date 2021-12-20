using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;



public class ImageLibraryConverter : MonoBehaviour
{

    [SerializeField]
    private XRReferenceImageLibrary lib;

    [SerializeField]
    private ARTrackedImageManager imageManager;

    private const string libraryPath = "Assets/ConvertedLibraries/";
    private RuntimeReferenceImageLibrary runtimeLib;


    // Update is called once per frame
    void Update()
    {
        runtimeLib = imageManager.CreateRuntimeLibrary(lib);
        
        //AssetDatabase.CreateAsset(runtimeLib, libraryPath + lib.name + ".asset");
        AssetDatabase.SaveAssets();
    }
}
