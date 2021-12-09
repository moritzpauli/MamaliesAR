using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ActiveImageSpinning : MonoBehaviour
{
    private Image spinner;


    private void Start()
    {
        spinner = GetComponent<Image>();
    }


    // Update is called once per frame
    void Update()
    {
        spinner.rectTransform.Rotate(new Vector3(0, 0, 500 * Time.deltaTime));
    }
}
