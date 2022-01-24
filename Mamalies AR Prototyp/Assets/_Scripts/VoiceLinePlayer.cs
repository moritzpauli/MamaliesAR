using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;

public class VoiceLinePlayer : MonoBehaviour
{
    private PersistentLanguageManager persistentLanguageManager;

    private AudioSource audioSource;
    private AudioClip currentLoadedClip;

    private float waitTime;


    [SerializeField]
    private float timeBetweenLanguages;

    private bool readingInProgress;

    private bool loadingData;

    private const string voiceLinePath = "Assets/_VoiceLines/";

    AsyncOperationHandle<AudioClip> clipHandle;

    private string selectedLanguageOne;
    private string selectedLanguageTwo;

    private bool clipPlaying = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        Addressables.InitializeAsync();
        persistentLanguageManager = GameObject.FindObjectOfType<PersistentLanguageManager>();

        selectedLanguageOne = persistentLanguageManager.GetLanguageOne();
        selectedLanguageTwo = persistentLanguageManager.GetLanguageTwo();
    }

    private void Update()
    {
        if (clipPlaying)
        {
            if (!audioSource.isPlaying)
            {
                clipPlaying = false;
                Addressables.Release(clipHandle.Result);
            }
        }
    }

    /// <summary>
    /// Starts Playboth languages if it is not already running
    /// </summary>
    /// <param name="vignetteName"></param>
    public void PlayVoiceLine(string vignetteName)
    {
        if (!audioSource.isPlaying && !readingInProgress)
        {
            StartCoroutine(PlayBothLanguages(vignetteName));

        }
    }

    /// <summary>
    /// Load addressable voiceline in specified language, insert it in audiosource and play
    /// </summary>
    /// <param name="Vignettename"></param>
    /// <param name="lang"></param>
    private void LoadAndPlayAdressableVoiceLine(string Vignettename, string lang)
    {
        if (lang.Length == 2)
        {
            loadingData = true;
            string filePath = voiceLinePath + lang + "/" + lang + '_' + Vignettename + ".mp3";
            clipHandle = Addressables.LoadAssetAsync<AudioClip>(filePath);
            clipHandle.Completed += (operation) =>
            {
                currentLoadedClip = clipHandle.Result;

                audioSource.clip = currentLoadedClip;
                audioSource.Play();
                clipPlaying = true;
                print("Play" + audioSource.clip.name);
                waitTime = audioSource.clip.length + timeBetweenLanguages;
                loadingData = false;
                //Addressables.Release(clipHandle);
            };
        }
    }


    /// <summary>
    /// checks if line is currently playing
    /// </summary>
    /// <returns></returns>
    public bool HasLinePlaying()
    {
        if (audioSource.isPlaying || readingInProgress)
        {
            return true;
        }
        else
        {
            return false;
        }
    }



    /// <summary>
    /// reads german first, waits then plays second language
    /// </summary>
    /// <param name="lineIndex"></param>
    /// <returns></returns>
    private IEnumerator PlayBothLanguages(string vignetteName)
    {
        readingInProgress = true;
        LoadAndPlayAdressableVoiceLine(vignetteName, selectedLanguageOne);
        yield return new WaitUntil(() => loadingData == false);
        if (selectedLanguageTwo != selectedLanguageOne && selectedLanguageTwo != null && selectedLanguageTwo != "")
        {
            yield return new WaitForSeconds(waitTime);
            LoadAndPlayAdressableVoiceLine(vignetteName, selectedLanguageTwo);
            yield return new WaitUntil(() => loadingData == false);
            readingInProgress = false;
            //Addressables.Release(clipHandle.Result);
        }
        else
        {
            readingInProgress = false;
            //Addressables.Release(clipHandle.Result);
        }
    }

    public void SetReadingInterval(float newInterval)
    {
        timeBetweenLanguages = newInterval;
    }
}
