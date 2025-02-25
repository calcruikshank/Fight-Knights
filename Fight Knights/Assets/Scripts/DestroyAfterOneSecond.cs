using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DestroyAfterOneSecond : NetworkBehaviour
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
        if (NetworkManager.Singleton != null && !IsServer)
        {
            return;
        }
        destroyTimer += Time.deltaTime;
        if (destroyTimer >= destroyTarget)
        {
            Destroy(this.gameObject);
        }
    }
}
