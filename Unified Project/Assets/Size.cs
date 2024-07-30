using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Size : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector3 meow = GetComponent<Renderer>().bounds.size;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
