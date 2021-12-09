using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TestRemoteAssetLoading : MonoBehaviour
{
    [SerializeField]
    private string assetPath;

    [SerializeField]
    private Image testImage;

    [SerializeField]
    private AssetReference assetReference;
    [SerializeField]
    private AssetReference cubeReference;

    AsyncOperationHandle<Sprite> testSprite;

    //private IEnumerator Start()
    //{
    //    Addressables.ClearDependencyCacheAsync(assetReference);

    //    //Check the download size
    //    AsyncOperationHandle<long> getDownloadSize = Addressables.GetDownloadSizeAsync(assetReference);
    //    yield return getDownloadSize;
    //    print(getDownloadSize);

    //    //If the download size is greater than 0, download all the dependencies.
    //    if (getDownloadSize.Result > 0)
    //    {
    //        AsyncOperationHandle downloadDependencies = Addressables.DownloadDependenciesAsync(assetReference);
    //        yield return downloadDependencies;
    //    }
    //}

    private void Start()
    {
        testImage.gameObject.SetActive(false);
        testSprite = Addressables.LoadAssetAsync<Sprite>(assetReference);

        testSprite.Completed += DisplaySprite;
        cubeReference.InstantiateAsync();

    }


    ////private void Start()
    ////{
    ////    cubeReference.InstantiateAsync();
    ////}

    private void Update()
    {
        if (testSprite.PercentComplete < 1f)
        {
            print(testSprite.PercentComplete);
        }
    }

    private void DisplaySprite(AsyncOperationHandle<Sprite> handle)
    {
        print(handle.Result.name);
        testImage.gameObject.SetActive(true);
        testImage.sprite = handle.Result;
    }
}
