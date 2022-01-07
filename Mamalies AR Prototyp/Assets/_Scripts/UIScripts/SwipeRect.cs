using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;



public class SwipeRect : MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField]
    private float percentageThreshold = 0.2f;
    [SerializeField]
    private float smoothing = 0.5f;
    [SerializeField]
    private int currentPage = 0;
    private Vector3 panelPosition;
    private Vector3 ogPanelPosition;
    [SerializeField]
    private float stepWidth = 0;

    private int pagesCount;


    void Start()
    {
        panelPosition = transform.position;
        ogPanelPosition = panelPosition;
        pagesCount = transform.childCount;
        //stepWidth = transform.parent.GetComponent<RectTransform>().sizeDelta.x;
        Canvas.ForceUpdateCanvases();
        StartCoroutine(GetStepWidth());
    }



    /// <summary>
    /// Called when drag starts
    /// </summary>
    /// <param name="pData"></param>
    public void OnDrag(PointerEventData pData)
    {
        float difference = pData.pressPosition.x - pData.position.x;
        transform.position = panelPosition - new Vector3(difference, 0, 0);
    }


    /// <summary>
    /// Called when pointer/finger is lifted after drag motion
    /// </summary>
    /// <param name="pData"></param>
    public void OnEndDrag(PointerEventData pData)
    {
        float dragPercentage = (pData.pressPosition.x - pData.position.x) / stepWidth;;
        if (Mathf.Abs(dragPercentage) > percentageThreshold)
        {
            Vector3 newPanelPosition = panelPosition;
            if (dragPercentage > 0 && currentPage < pagesCount - 1)
            {
                currentPage++;
                newPanelPosition += new Vector3(-stepWidth, 0, 0);

            }
            else if (dragPercentage < 0 && currentPage > 0)
            {
                currentPage--;
                newPanelPosition += new Vector3(stepWidth, 0, 0);
            }
            StartCoroutine(SmoothMovement(transform.position, newPanelPosition, smoothing));
            panelPosition = newPanelPosition;
        }
        else
        {
            StartCoroutine(SmoothMovement(transform.position, panelPosition, smoothing));
        }
    }

    /// <summary>
    /// Smooth the drag motion
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="endPos"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    private IEnumerator SmoothMovement(Vector3 startPos, Vector3 endPos, float time)
    {
        float passedTime = 0f;
        while (passedTime <= 1f)
        {
            passedTime += Time.deltaTime / time;
            transform.position = Vector3.Lerp(startPos, endPos, Mathf.SmoothStep(0f, 1f, passedTime));
            yield return null;
        }
        if(currentPage > pagesCount - 2)
        {
            transform.position = ogPanelPosition;
            panelPosition = ogPanelPosition;
            currentPage = 0;
        }
        yield return null;

    }
    private IEnumerator GetStepWidth()
    {
        yield return new WaitForEndOfFrame();
        stepWidth = Mathf.Abs(transform.GetChild(0).GetComponent<RectTransform>().position.x-transform.GetChild(1).GetComponent<RectTransform>().position.x);
    }
}