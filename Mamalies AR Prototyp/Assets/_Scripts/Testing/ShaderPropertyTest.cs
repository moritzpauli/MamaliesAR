using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShaderPropertyTest : MonoBehaviour
{
    [SerializeField]
    private Image viewFinder;
    [SerializeField]
    private Slider alpha;
    [SerializeField]
    private Slider blur;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //viewFinder.material.SetFloat("_BlurAmount", 0.5f);
        //viewFinder.material.SetFloat("_ResultAlpha", 0.3f);
        //viewFinder.material.SetFloat("_ResultAlpha", alpha.value);
        //viewFinder.material.SetFloat("_BlurAmount", blur.value);
    }

    public void SetAlpha(float val)
    {
        viewFinder.material.SetFloat("_ResultAlpha",val);
    }

    public void SetBlur(float val)
    {
        viewFinder.material.SetFloat("_BlurAmount",val);
    }
}
