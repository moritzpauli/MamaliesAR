using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

	private void Start()
	{
		rTransform = GetComponent<RectTransform>();
		originalScale = rTransform.localScale.x;
		textMesh = GetComponent<TextMeshProUGUI>();
		textMesh.enabled = false;

	}

	public void PlayRecognisedAnimation()
	{
		StopAllCoroutines();
        StartCoroutine(AnimateRect());
	}

    private IEnumerator AnimateRect()
	{
		scannedImageBackground.gameObject.SetActive(true);
		float timer = 0;
		rTransform.localScale = new Vector3(originalScale, originalScale, originalScale);
		textMesh.alpha = 1.0f;
		textMesh.enabled = true;
		while(timer < animationDuration)
		{
			rTransform.localScale = Vector3.Lerp(new Vector3(originalScale,originalScale,originalScale),new Vector3(targetScale,targetScale,targetScale),timer/animationDuration);
			timer += Time.deltaTime;
			if(timer < animationDuration * 0.8f)
			{
				textMesh.alpha = Mathf.Lerp(1.0f, 0,timer-animationDuration*0.8f/animationDuration*0.2f);
			}
			yield return null;
		}	
		textMesh.enabled = false;
		yield return null;
		scannedImageBackground.gameObject.SetActive(false);
	}
}
