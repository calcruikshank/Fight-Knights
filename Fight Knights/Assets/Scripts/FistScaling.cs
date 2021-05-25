using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FistScaling : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale = new Vector3((transform.localPosition.x) + 2, (transform.localPosition.x) + 1, (transform.localPosition.x) + 2);
    }
}
