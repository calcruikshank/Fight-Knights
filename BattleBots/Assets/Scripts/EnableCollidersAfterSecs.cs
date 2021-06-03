using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableCollidersAfterSecs : MonoBehaviour
{
    Collider[] colliders;
    [SerializeField] float enableAfterSecs = .75f;
    float timer = 0f;
    // Start is called before the first frame update
    void Start()
    {
        timer = 0f;
        colliders = this.gameObject.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > enableAfterSecs)
        {
            foreach (Collider collider in colliders)
            {
                collider.enabled = true;
            }
        }
    }
}
