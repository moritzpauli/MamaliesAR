using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

public class RecognisedAnimation : MonoBehaviour
{
    [SerializeField]
    private float animationDuration = 1.5f;


    [SerializeField]
    private float targetScale = 2.4f;


    private float originalScale;
	private RectTransform rTransform;
	private TextMeshProUGUI textMesh;

	[SerializeField]
	private Image scannedImageBackground;

	[SerializeField]
	private bool useJobs;

	[SerializeField]
	private bool displayBackground = false;

	private void Start()
	{
		rTransform = GetComponent<RectTransform>();
		originalScale = rTransform.localScale.x;
		textMesh = GetComponent<TextMeshProUGUI>();
		textMesh.enabled = false;

	}
                                                                                                                                                                                                                                                                                                                                                                                                                          

    private void Update()
    {
		float startTime = Time.realtimeSinceStartup;
		//LoadTestTask();
		LoadTestTaskJob();
		//Debug.Log((Time.realtimeSinceStartup - startTime) * 1000 + "ms");
    }

	private void LoadTestTask()
    {
		float value = 0f;
		for(int i = 0; i < 500000; i++)
        {
			value = math.exp10(math.sqrt(value));
        }
    }

	private JobHandle LoadTestTaskJob()
    {
		LoadTestJob job = new LoadTestJob();
		return job.Schedule();
    }
	

    public void PlayRecognisedAnimation()
	{
		StopAllCoroutines();
		StartCoroutine(AnimateRect());
		//textMesh.enabled = true;
		//Task rotateTask = RotateAsync();
		

	}

    private IEnumerator AnimateRect()
	{
		if (displayBackground)
		{
			scannedImageBackground.gameObject.SetActive(true);
		}
		float timer = 0;
		rTransform.localScale = new Vector3(originalScale, originalScale, originalScale);
		textMesh.alpha = 1.0f;
		textMesh.enabled = true;
		
		while (timer < animationDuration)
		{
			timer += Time.deltaTime;
			rTransform.localScale = Vector3.Lerp(new Vector3(originalScale,originalScale,originalScale),new Vector3(targetScale,targetScale,targetScale),timer/animationDuration);
			
			if(timer > animationDuration * 0.8f)
			{
				textMesh.alpha = Mathf.Lerp(1.0f, 0f, (timer - animationDuration * 0.8f) / (animationDuration * 0.2f));
			
				
			}
			yield return null;
		}	
		textMesh.enabled = false;
		if (displayBackground)
		{
			scannedImageBackground.gameObject.SetActive(false);
		}
		yield return null;
		
	}


    #region AsyncFunctions

	private async Task RotateAsync()
    {
		float timer = 4.0f;
		while (timer > 0)
		{
			textMesh.transform.eulerAngles += new Vector3(Time.deltaTime*10, Time.deltaTime*10, Time.deltaTime*10);
			timer -= Time.deltaTime;
			await Task.Yield();

		}
		await Task.Yield();
    }

    #endregion
}

public struct LoadTestJob : IJob
{
	public void Execute()
    {
		float value = 0f;
		for (int i = 0; i < 500000; i++)
		{
			value = math.exp10(math.sqrt(value));
		}
	}
}


public struct AnimateMessageJob : IJob
{


	private float rotateTimer;
	public void Execute()
    {
		rotateTimer = 2.0f;
		while (rotateTimer > 0)
		{
			GameObject text = GameObject.FindGameObjectWithTag("pageMessage");
			text.transform.eulerAngles += new Vector3(Time.deltaTime, Time.deltaTime, Time.deltaTime);
			rotateTimer -= Time.deltaTime;
			Debug.Log(rotateTimer);
		}
    }
}
