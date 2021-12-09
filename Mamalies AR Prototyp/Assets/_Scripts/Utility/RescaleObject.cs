using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RescaleObject : MonoBehaviour
{

    private Vector3 ogScale;
    private void Start()
    {
        ogScale = transform.localScale;
    }

    public void ScaleGameObject(float scaleMultiplier)
    {
        transform.localScale = new Vector3(ogScale.x*scaleMultiplier,ogScale.y*scaleMultiplier,ogScale.z*scaleMultiplier);
    }
}
