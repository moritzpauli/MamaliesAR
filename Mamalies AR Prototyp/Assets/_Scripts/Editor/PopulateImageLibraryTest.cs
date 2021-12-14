using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEditor.XR.ARSubsystems;
using UnityEditor.XR;
using UnityEditor.EditorTools;
using System.IO;




public class PopulateImageLibraryTest : Editor
{


    private static XRReferenceImageLibrary imageLibrary;
    private static string libraryPath = "Assets/_TrackingVignettes/ImageLibrary/VignetteLibrary.asset";
    private static string iOSLibraryFolder = "Assets/_TrackingVignettes/ImageLibrary/VignetteLibrariesIOS/";
    private static string rootImagePath = "Assets/_TrackingVignettes";
    private static string iOSLibraryName = "iOSImageLibrary_";
    private static string[] imagePaths;

    private static List<Texture2D> textures = new List<Texture2D>();


    //[MenuItem("ImgLibrary/Remove Testing Images")]
    //static void RemoveTestingImages()
    //{
    //    imageLibrary = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(libraryPath);
    //    int imagelibraryCount = imageLibrary.count-1;
    //    for (int i = 0; i < imagelibraryCount; i++)
    //    {
    //        if (imageLibrary[i] == null || imageLibrary[i].texture == null)
    //        {
    //            imageLibrary.RemoveAt(i);
    //        }
    //        else if (imageLibrary[i].texture.name[0] == '#')
    //        {
    //            imageLibrary.RemoveAt(i);
    //        }
    //    }
    //}

    //[MenuItem("ImgLibrary/Remove Production Images")]
    //static void RemoveProductionImages()
    //{
    //    imageLibrary = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(libraryPath);
    //    int imagelibraryCount = imageLibrary.count;
    //    for(int i = 0; i < imagelibraryCount; i++)
    //    {
    //        if (imageLibrary[i] == null || imageLibrary[i].texture == null)
    //        {
    //            imageLibrary.RemoveAt(i);
    //        }
    //        else if (imageLibrary[i].texture.name[0] == '0')
    //        {
    //            imageLibrary.RemoveAt(i);
    //        }
    //    }
        
    //}

    [MenuItem("ImgLibrary/Remove All Images(Recommended)")]
    static void RemoveAllImages()
    {
        imageLibrary = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(libraryPath);
        while (imageLibrary.count > 0)
        {
            imageLibrary.RemoveAt(imageLibrary.count - 1);
        }

    }


    //[MenuItem("ImgLibrary/Autopopulate with Testing Images")]
    //static void AutopopulateLibraryTesting()
    //{
    //    imageLibrary = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(libraryPath);


    //    imagePaths = Directory.GetFiles(rootImagePath +"/TestingImages","*.png");

    //    foreach (string imageFilePath in imagePaths)
    //    {
    //        //Debug.Log(imageFilePath);
    //        Texture2D tempTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(imageFilePath, typeof(Texture2D));

    //        if (Char.IsDigit(tempTexture.name[0]))
    //        {
    //            textures.Add(tempTexture);
    //        }


    //    }

    //    Debug.Log("New Textures: " + textures.Count);

    //    for (int i = imageLibrary.count - 1; i >= 0; i--)
    //    {
    //        if (imageLibrary[i].texture.name[0] == '#')
    //        {
    //            imageLibrary.RemoveAt(i);
    //        }
    //    }



    //    foreach (Texture2D texture in textures)
    //    {
    //        imageLibrary.Add();
    //        imageLibrary.SetTexture(imageLibrary.count-1, texture, false);
    //        imageLibrary.SetSpecifySize(imageLibrary.count - 1, true);
    //        imageLibrary.SetSize(imageLibrary.count - 1, new Vector2((float)texture.width/7f * 0.001f,(float)texture.height/7f * 0.001f));
    //        imageLibrary.SetName(imageLibrary.count-1,'#' + texture.name);
    //    }

    //    Debug.Log("Image Library Size:" + imageLibrary.count);
    //    textures.Clear();
    //}


    [MenuItem("ImgLibrary/Autopopulate image Libraries IOS")]
    static void AutopopulateLibraryProductionIOS()
    {
        imagePaths = Directory.GetFiles(rootImagePath + "/ProductionImages", "*.png");

        int imageCounter = 0;

        int libraryCounter = 0;

        XRReferenceImageLibrary currentLibrary = new XRReferenceImageLibrary();

        textures.Clear();

        foreach (string imageFilePath in imagePaths)
        {
            //Debug.Log(imageFilePath);
            Texture2D tempTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(imageFilePath, typeof(Texture2D));
            if (Char.IsDigit(tempTexture.name[0]))
            {
                textures.Add(tempTexture);
            }

        }

        for(int i = 0; i < textures.Count; i++)
        {
            
            if(imageCounter <= 0)
            {
                currentLibrary = new XRReferenceImageLibrary();
                AssetDatabase.CreateAsset(currentLibrary, iOSLibraryFolder + iOSLibraryName+ libraryCounter.ToString() + ".asset");
                imageCounter = 200;
                libraryCounter++;
            }
            currentLibrary.Add();
            currentLibrary.SetTexture(currentLibrary.count - 1, textures[i], false);
            currentLibrary.SetSpecifySize(currentLibrary.count - 1, true);
            currentLibrary.SetSize(currentLibrary.count - 1, new Vector2((float)textures[i].width / 7f * 0.001f, (float)textures[i].height / 7f * 0.001f));
            currentLibrary.SetName(currentLibrary.count - 1, textures[i].name);
            imageCounter--;


        }

        AssetDatabase.SaveAssets();
    }


    [MenuItem("ImgLibrary/Autopopulate with 25 Production Images")]
    static void AutopopulateLibraryProductionNumber()
    {
        imageLibrary = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(libraryPath);


        imagePaths = Directory.GetFiles(rootImagePath + "/ProductionImages", "*.png");

        foreach (string imageFilePath in imagePaths)
        {
            //Debug.Log(imageFilePath);
            Texture2D tempTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(imageFilePath, typeof(Texture2D));
            if (Char.IsDigit(tempTexture.name[0]))
            {
                textures.Add(tempTexture);
            }

        }

        Debug.Log("New Textures: " + textures.Count);

        for (int i = imageLibrary.count - 1; i >= 0; i--)
        {
            if (imageLibrary[i].texture.name[0] == '0')
            {
                imageLibrary.RemoveAt(i);
            }
        }

        for(int i = 0; i < 25; i++)
        {
            imageLibrary.Add();
            imageLibrary.SetTexture(imageLibrary.count - 1, textures[i], false);
            imageLibrary.SetSpecifySize(imageLibrary.count - 1, true);
            imageLibrary.SetSize(imageLibrary.count - 1, new Vector2((float)textures[i].width / 7f * 0.001f, (float)textures[i].height / 7f * 0.001f));
            imageLibrary.SetName(imageLibrary.count - 1, textures[i].name);
        }

       

        Debug.Log("Image Library Size:" + imageLibrary.count);
        textures.Clear();
    }


    [MenuItem("ImgLibrary/Autopopulate with Production Images")]
    static void AutopopulateLibraryProduction()
    {
        imageLibrary = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(libraryPath);


        imagePaths = Directory.GetFiles(rootImagePath + "/ProductionImages", "*.png");

        foreach (string imageFilePath in imagePaths)
        {
            //Debug.Log(imageFilePath);
            Texture2D tempTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(imageFilePath, typeof(Texture2D));
            if (Char.IsDigit(tempTexture.name[0]))
            {
                textures.Add(tempTexture);
            }

        }

        Debug.Log("New Textures: " + textures.Count);

        for (int i = imageLibrary.count - 1; i >= 0; i--)
        {
            if (imageLibrary[i].texture.name[0] == '0')
            {
                imageLibrary.RemoveAt(i);
            }
        }



        foreach (Texture2D texture in textures)
        {
            imageLibrary.Add();
            imageLibrary.SetTexture(imageLibrary.count - 1, texture, false);
            imageLibrary.SetSpecifySize(imageLibrary.count - 1, true);
            imageLibrary.SetSize(imageLibrary.count - 1, new Vector2((float)texture.width / 7f * 0.001f, (float)texture.height / 7f * 0.001f));
            imageLibrary.SetName(imageLibrary.count - 1, texture.name);
        }

        Debug.Log("Image Library Size:" + imageLibrary.count);
        textures.Clear();
    }


    [MenuItem("ImgLibrary/Autopopulate with persistent Production Images")]
    static void AutopopulateLibraryProductionKeepTextures()
    {
        imageLibrary = AssetDatabase.LoadAssetAtPath<XRReferenceImageLibrary>(libraryPath);


        imagePaths = Directory.GetFiles(rootImagePath + "/ProductionImages", "*.png");

        foreach (string imageFilePath in imagePaths)
        {
            //Debug.Log(imageFilePath);
            Texture2D tempTexture = (Texture2D)AssetDatabase.LoadAssetAtPath(imageFilePath, typeof(Texture2D));
            if (Char.IsDigit(tempTexture.name[0]))
            {
                textures.Add(tempTexture);
            }

        }

        Debug.Log("New Textures: " + textures.Count);

        for (int i = imageLibrary.count - 1; i >= 0; i--)
        {
            if (imageLibrary[i].texture.name[0] == '0')
            {
                imageLibrary.RemoveAt(i);
            }
        }



        foreach (Texture2D texture in textures)
        {
            imageLibrary.Add();
            imageLibrary.SetTexture(imageLibrary.count - 1, texture, true);
            imageLibrary.SetSpecifySize(imageLibrary.count - 1, true);
            imageLibrary.SetSize(imageLibrary.count - 1, new Vector2((float)texture.width / 7f * 0.001f, (float)texture.height / 7f * 0.001f));
            imageLibrary.SetName(imageLibrary.count - 1, texture.name);
        }

        Debug.Log("Image Library Size:" + imageLibrary.count);
        textures.Clear();
    }

}
