using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UIRaycast : MonoBehaviour
{


    private GraphicRaycaster graphicRaycaster;
    private PointerEventData pointerEventData;
    [SerializeField]
    private EventSystem eventSystem;
    private List<RaycastResult> results = new List<RaycastResult>();

    // Start is called before the first frame update
    void Start()
    {
        graphicRaycaster = GetComponent<GraphicRaycaster>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            pointerEventData = new PointerEventData(eventSystem);
            pointerEventData.position = touch.position;
            results.Clear();
            graphicRaycaster.Raycast(pointerEventData, results);

            //foreach(RaycastResult result in results)
            //{
            //    print("Hit UI Object: " + result.gameObject.name);
            //}

            print(results[0].gameObject.name);

        }
    }
}
