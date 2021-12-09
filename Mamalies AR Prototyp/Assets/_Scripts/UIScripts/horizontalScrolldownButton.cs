using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class horizontalScrolldownButton : MonoBehaviour
{
    [SerializeField]
    private float scrollSpeed = 6.5f;

    private bool bScrolling = false;

    private ScrollRect scrollRect;

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    private void Update()
    {
        if (bScrolling)
        {
            scrollRect.verticalNormalizedPosition -= Time.deltaTime * scrollSpeed;
            if(scrollRect.verticalNormalizedPosition <= 0)
            {
                bScrolling = false;
            }
        }
    }

    public void ScrollDownToBottom()
    {
        bScrolling = true;
    }

}
