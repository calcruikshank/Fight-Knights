using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterOneSecond : MonoBehaviour
{
    public float destroyTimer;
    public float destroyTarget = 1f;
    // Start is called before the first frame update
    void Start()
    {
        destroyTimer = 0;
    }

    // Update is called once per frame
    void Update()
    {
        destroyTimer += Time.deltaTime;
        if (destroyTimer >= destroyTarget)
        {
            Destroy(this.gameObject);
        }
    }
}
