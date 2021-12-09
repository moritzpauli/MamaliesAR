using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slowdown : MonoBehaviour
{
    public int spawnAmount;

    [SerializeField]
    private GameObject spawnObject;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i < spawnAmount; i++)
        {
            Instantiate(spawnObject);
        }
    }
}
